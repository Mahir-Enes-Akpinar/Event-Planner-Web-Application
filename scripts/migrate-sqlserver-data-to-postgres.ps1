param(
    [string]$SqlContainer = "eventplanner-sql-legacy",
    [string]$PgContainer = "event-planner-web-application-postgres-1",
    [string]$SqlDatabase = "EventPlanner",
    [string]$PgDatabase = "EventPlanner",
    [string]$PgUser = "postgres"
)

$ErrorActionPreference = "Stop"

function Get-DotEnvValue([string]$Key) {
    if (-not (Test-Path ".env")) {
        return $null
    }

    $line = Get-Content ".env" | Where-Object { $_ -match "^$([regex]::Escape($Key))=" } | Select-Object -First 1
    if (-not $line) {
        return $null
    }

    return ($line -split "=", 2)[1]
}

function Invoke-SqlCmd([string]$Query) {
    docker exec $SqlContainer /opt/mssql-tools18/bin/sqlcmd `
        -S localhost `
        -U sa `
        -P $script:SqlPassword `
        -C `
        -d $SqlDatabase `
        -y 0 `
        -Y 0 `
        -w 65535 `
        -Q $Query
}

function Invoke-PgSql([string]$Sql) {
    docker exec $PgContainer psql -U $PgUser -d $PgDatabase -v ON_ERROR_STOP=1 -q -c $Sql
}

function Quote-PgIdent([string]$Name) {
    return '"' + $Name.Replace('"', '""') + '"'
}

function Convert-ToCopyText($Value, [bool]$IsBinary) {
    if ($null -eq $Value) {
        return "\N"
    }

    if ($IsBinary) {
        if ($Value -is [byte[]]) {
            $bytes = $Value
        }
        else {
            $bytes = [Convert]::FromBase64String([string]$Value)
        }

        $hex = -join ($bytes | ForEach-Object { $_.ToString("x2") })
        return "\\x$hex"
    }

    if ($Value -is [bool]) {
        if ($Value) { return "true" }
        return "false"
    }

    if ($Value -is [DateTime]) {
        return $Value.ToString("yyyy-MM-dd HH:mm:ss.fffffff", [Globalization.CultureInfo]::InvariantCulture)
    }

    $text = [string]$Value
    $text = $text.Replace("\", "\\")
    $text = $text.Replace("`t", "\t")
    $text = $text.Replace("`r", "\r")
    $text = $text.Replace("`n", "\n")
    return $text
}

$script:SqlPassword = Get-DotEnvValue "SQL_SA_PASSWORD"
if ([string]::IsNullOrWhiteSpace($script:SqlPassword)) {
    throw "SQL_SA_PASSWORD was not found in .env."
}

$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$exportRoot = Join-Path $PWD "tmp\sqlserver-to-postgres-$timestamp"
$backupRoot = Join-Path $PWD "backups"
New-Item -ItemType Directory -Force -Path $exportRoot | Out-Null
New-Item -ItemType Directory -Force -Path $backupRoot | Out-Null

$backupName = "postgres-before-sqlserver-import-$timestamp.sql"
$backupPath = Join-Path $backupRoot $backupName

Write-Host "Creating PostgreSQL backup: $backupPath"
docker exec $PgContainer pg_dump -U $PgUser -d $PgDatabase -f "/tmp/$backupName"
docker cp "${PgContainer}:/tmp/$backupName" $backupPath

$tables = @(
    @{ Name = "AspNetRoles"; Pk = @("Id") },
    @{ Name = "Kategoriler"; Pk = @("Id") },
    @{ Name = "Kullanicilar"; Pk = @("Id"); PreSql = @'
DELETE FROM "AspNetUserRoles" ur
USING "Kullanicilar" u, tmp_import t
WHERE ur."UserId" = u."Id"
  AND u."Id" <> t."Id"
  AND (
      u."NormalizedUserName" IS NOT DISTINCT FROM t."NormalizedUserName"
      OR u."NormalizedEmail" IS NOT DISTINCT FROM t."NormalizedEmail"
  );
DELETE FROM "AspNetUserTokens" tok
USING "Kullanicilar" u, tmp_import t
WHERE tok."UserId" = u."Id"
  AND u."Id" <> t."Id"
  AND (
      u."NormalizedUserName" IS NOT DISTINCT FROM t."NormalizedUserName"
      OR u."NormalizedEmail" IS NOT DISTINCT FROM t."NormalizedEmail"
  );
DELETE FROM "AspNetUserClaims" cl
USING "Kullanicilar" u, tmp_import t
WHERE cl."UserId" = u."Id"
  AND u."Id" <> t."Id"
  AND (
      u."NormalizedUserName" IS NOT DISTINCT FROM t."NormalizedUserName"
      OR u."NormalizedEmail" IS NOT DISTINCT FROM t."NormalizedEmail"
  );
DELETE FROM "AspNetUserLogins" lg
USING "Kullanicilar" u, tmp_import t
WHERE lg."UserId" = u."Id"
  AND u."Id" <> t."Id"
  AND (
      u."NormalizedUserName" IS NOT DISTINCT FROM t."NormalizedUserName"
      OR u."NormalizedEmail" IS NOT DISTINCT FROM t."NormalizedEmail"
  );
DELETE FROM "Kullanicilar" u
USING tmp_import t
WHERE u."Id" <> t."Id"
  AND (
      u."NormalizedUserName" IS NOT DISTINCT FROM t."NormalizedUserName"
      OR u."NormalizedEmail" IS NOT DISTINCT FROM t."NormalizedEmail"
  );
'@ },
    @{ Name = "AspNetUserClaims"; Pk = @("Id") },
    @{ Name = "AspNetUserLogins"; Pk = @("LoginProvider", "ProviderKey") },
    @{ Name = "AspNetUserRoles"; Pk = @("UserId", "RoleId") },
    @{ Name = "AspNetUserTokens"; Pk = @("UserId", "LoginProvider", "Name") },
    @{ Name = "Etkinlikler"; Pk = @("Id") },
    @{ Name = "EtkinlikKategorileri"; Pk = @("EtkinlikId", "KategoriId") },
    @{ Name = "Katilimlar"; Pk = @("Id") },
    @{ Name = "Mesajlar"; Pk = @("Id") },
    @{ Name = "Puanlar"; Pk = @("Id") },
    @{ Name = "EventStaff"; Pk = @("Id") },
    @{ Name = "AttendanceQrTokens"; Pk = @("Id") },
    @{ Name = "AuditLogs"; Pk = @("Id") },
    @{ Name = "EmailOutbox"; Pk = @("Id") }
)

foreach ($table in $tables) {
    $tableName = $table.Name
    Write-Host "Migrating $tableName..."

    $columnQuery = @"
SET NOCOUNT ON;
SELECT COLUMN_NAME + '|' + DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = '$tableName'
ORDER BY ORDINAL_POSITION;
"@

    $columnLines = Invoke-SqlCmd $columnQuery | Where-Object { $_ -match "\|" }
    if (-not $columnLines) {
        Write-Host "  skipped; source table does not exist."
        continue
    }

    $columns = @()
    $binaryColumns = @{}
    foreach ($line in $columnLines) {
        $parts = $line -split "\|", 2
        $columnName = $parts[0].Trim()
        $dataType = $parts[1].Trim().ToLowerInvariant()
        $columns += $columnName
        if ($dataType -in @("binary", "varbinary", "image", "rowversion", "timestamp")) {
            $binaryColumns[$columnName] = $true
        }
    }

    $jsonQuery = @"
SET NOCOUNT ON;
SELECT (
    SELECT *
    FROM dbo.[$tableName]
    FOR JSON PATH, INCLUDE_NULL_VALUES
) AS JsonData;
"@

    $json = (Invoke-SqlCmd $jsonQuery | Where-Object {
        $_ -and $_.Trim().StartsWith("[")
    }) -join ""
    $json = $json.Trim()
    if ([string]::IsNullOrWhiteSpace($json) -or $json -eq "NULL") {
        Write-Host "  no rows."
        continue
    }

    $rows = $json | ConvertFrom-Json
    if ($null -eq $rows) {
        Write-Host "  no rows."
        continue
    }

    if ($rows -isnot [System.Array]) {
        $rows = @($rows)
    }

    $tsvPath = Join-Path $exportRoot "$tableName.tsv"
    $lines = New-Object System.Collections.Generic.List[string]
    foreach ($row in $rows) {
        $values = foreach ($column in $columns) {
            $value = $row.PSObject.Properties[$column].Value
            Convert-ToCopyText $value $binaryColumns.ContainsKey($column)
        }
        $lines.Add(($values -join "`t"))
    }
    [System.IO.File]::WriteAllLines($tsvPath, $lines, [System.Text.UTF8Encoding]::new($false))

    docker exec $PgContainer mkdir -p /tmp/sqlserver-import
    if ($LASTEXITCODE -ne 0) { throw "Failed to create import directory in PostgreSQL container." }
    docker cp $tsvPath "${PgContainer}:/tmp/sqlserver-import/$tableName.tsv"
    if ($LASTEXITCODE -ne 0) { throw "Failed to copy TSV for $tableName." }

    $quotedTable = Quote-PgIdent $tableName
    $quotedColumns = ($columns | ForEach-Object { Quote-PgIdent $_ }) -join ", "
    $quotedPk = ($table.Pk | ForEach-Object { Quote-PgIdent $_ }) -join ", "
    $nonPkColumns = $columns | Where-Object { $table.Pk -notcontains $_ }
    if ($nonPkColumns.Count -gt 0) {
        $updateSet = ($nonPkColumns | ForEach-Object {
            $q = Quote-PgIdent $_
            "$q = EXCLUDED.$q"
        }) -join ", "
        $conflictSql = "ON CONFLICT ($quotedPk) DO UPDATE SET $updateSet"
    }
    else {
        $conflictSql = "ON CONFLICT ($quotedPk) DO NOTHING"
    }

    $preSql = ""
    if ($table.ContainsKey("PreSql")) {
        $preSql = $table.PreSql
    }

    $sequenceSql = ""
    if ($columns -contains "Id") {
        $sequenceSql = "SELECT setval(pg_get_serial_sequence('$quotedTable', 'Id'), GREATEST((SELECT COALESCE(MAX(""Id""), 1) FROM $quotedTable), 1), true);"
    }

    $importSql = @"
BEGIN;
CREATE TEMP TABLE tmp_import (LIKE $quotedTable INCLUDING DEFAULTS);
COPY tmp_import ($quotedColumns)
FROM '/tmp/sqlserver-import/$tableName.tsv'
WITH (FORMAT text, DELIMITER E'\t', NULL '\N');
$preSql
INSERT INTO $quotedTable ($quotedColumns)
SELECT $quotedColumns FROM tmp_import
$conflictSql;
DROP TABLE tmp_import;
$sequenceSql
COMMIT;
"@

    $sqlPath = Join-Path $exportRoot "$tableName.import.sql"
    [System.IO.File]::WriteAllText($sqlPath, $importSql, [System.Text.UTF8Encoding]::new($false))
    docker cp $sqlPath "${PgContainer}:/tmp/sqlserver-import/$tableName.import.sql"
    if ($LASTEXITCODE -ne 0) { throw "Failed to copy import SQL for $tableName." }
    docker exec $PgContainer psql -U $PgUser -d $PgDatabase -v ON_ERROR_STOP=1 -q -f "/tmp/sqlserver-import/$tableName.import.sql"
    if ($LASTEXITCODE -ne 0) { throw "PostgreSQL import failed for $tableName." }

    Write-Host "  imported $($rows.Count) rows."
}

Write-Host "Done. Backup saved at: $backupPath"
