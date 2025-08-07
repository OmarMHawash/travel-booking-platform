# Example SQL Server 2022 on Linux image base
FROM mcr.microsoft.com/mssql/server:2022-latest
ENV ACCEPT_EULA=Y
# Password provided at runtime via env
# Expose only internal port (no host publish in compose)
EXPOSE 1433

# Optional: add tools if not present
# mcr image typically includes sqlcmd in tools18; verify path in your base
# HEALTHCHECK will be in compose to use env var