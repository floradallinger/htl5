#!/usr/bin/env bash

MIGRATION_PROJECT="${1:-./PuppyAdoption.Persistence/PuppyAdoption.Persistence.csproj}"
STARTUP_PROJECT="${2:-./PuppyAdoption/PuppyAdoption.csproj}"

# Regex für Migrationsnamen: überspringt die [dbg]-Logs, Migrationen beginnen mit einem 14-stelligen Datum
MIGRATION_NAME_REGEX='^[0-9]{14}_.+'
exit_code=0
last_rc=0

assert_exit_code() {
    last_rc=$1
    if [ "$last_rc" -ne 0 ]; then
        exit_code=$last_rc
    fi
}

get_migrations() {
    dotnet ef migrations list \
        --project "$MIGRATION_PROJECT" \
        --startup-project "$STARTUP_PROJECT" | grep -E "$MIGRATION_NAME_REGEX"
}

add_migration() {
    local migration_name="$1"

    if [ -z "${migration_name// /}" ]; then
        migration_name="Initial"
    fi

    echo
    echo "Adding migration '$migration_name'..."
    dotnet ef migrations add "$migration_name" \
        --project "$MIGRATION_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
    assert_exit_code $?
}

update_database() {
    echo
    echo "Updating the database..."
    dotnet ef database update \
        --project "$MIGRATION_PROJECT" \
        --startup-project "$STARTUP_PROJECT"
    assert_exit_code $?
}

script_migration() {
    local from_migration="${1:-0}"
    local to_migration="$2"
    local output_path="$3"

    if [ -z "${to_migration// /}" ]; then
        to_migration=$(get_migrations | tail -n 1)
    fi

    if [ -z "${output_path// /}" ]; then
        output_path="./migration.sql"
    fi

    echo
    echo "Generating SQL script from migration '$from_migration' to '$to_migration'..."
    dotnet ef migrations script "$from_migration" "$to_migration" \
        --project "$MIGRATION_PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        --output "$output_path"
    assert_exit_code $?

    if [ "$exit_code" -eq 0 ]; then
        echo "SQL script written to $output_path"
    fi
}

list_migrations() {
    echo
    echo "Available migrations:"
    echo "----------------------------------"
    local i=0
    local migration
    while IFS= read -r migration; do
        [ -z "$migration" ] && continue
        echo "$i. $migration"
        i=$((i + 1))
    done < <(get_migrations)
    if [ "$i" -eq 0 ]; then
        echo "No migrations found."
    fi
    echo "----------------------------------"
}

echo "=================================="
echo "   EF Core Migration Management   "
echo "=================================="
echo "1) Add Migration"
echo "2) Update Database"
echo "3) Add + Update"
echo "4) Generate SQL Migration Script"
echo "5) List Migrations"
echo "----------------------------------"

read -r -p "Please enter 1, 2, 3, 4 or 5: " choice

case "$choice" in
    1)
        read -r -p "Enter the migration name (leave blank for 'Initial'): " migration_name
        add_migration "$migration_name"
            ;;
    2)
        update_database
        ;;
    3)
        read -r -p "Enter the migration name (leave blank for 'Initial'): " migration_name
        add_migration "$migration_name"
        if [ "$last_rc" -eq 0 ]; then
            echo "--------------------------------------------------------------------"
            update_database
        else
            echo "--------------------------------------------------------------------"
            echo "Migration failed. Database update skipped."
        fi
        ;;
    4)
        echo
        echo "Generating SQL Migration Script..."
        list_migrations
        read -r -p "Enter the start migration name (leave blank for '0'): " from_migration
        read -r -p "Enter the target migration name (leave blank for latest): " to_migration
        read -r -p "Enter output SQL file path (leave blank for './migration.sql'): " output_path
        script_migration "$from_migration" "$to_migration" "$output_path"
        ;;
    5)
        list_migrations
        ;;
    *)
        echo
        echo "Invalid selection. Exiting..."
        ;;
esac

if [ "$exit_code" -ne 0 ]; then
    echo "Failed! Exit code: $exit_code" >&2
else
    echo
    echo "Done!"
fi

exit "$exit_code"