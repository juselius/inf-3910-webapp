FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
RUN rm /etc/ssl/openssl.cnf
WORKDIR /app
COPY deploy/ ./
CMD dotnet Server.dll
