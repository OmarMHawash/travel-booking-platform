# Start from the official Microsoft SQL Server image
FROM mcr.microsoft.com/mssql/server:2022-latest

# Switch to the root user to install packages
USER root

# Install required packages and the Microsoft ODBC driver and SQL Server tools
RUN apt-get update && \
    apt-get install -y gnupg curl apt-transport-https && \
    curl https://packages.microsoft.com/keys/microsoft.asc | apt-key add - && \
    curl https://packages.microsoft.com/config/debian/11/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    curl https://packages.microsoft.com/config/ubuntu/22.04/prod.list > /etc/apt/sources.list.d/mssql-release.list && \
    apt-get update && \
    ACCEPT_EULA=Y apt-get install -y msodbcsql18 mssql-tools18 && \
    rm -rf /var/lib/apt/lists/* && \
    echo 'export PATH="$PATH:/opt/mssql-tools18/bin"' >> /etc/profile

# Switch back to the mssql user
USER mssql