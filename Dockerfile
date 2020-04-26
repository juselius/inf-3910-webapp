FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY ./charts/trust.crt /usr/local/share/ca-certificates
RUN update-ca-certificates
RUN rm /etc/ssl/openssl.cnf
WORKDIR /app
COPY deploy/ ./
CMD dotnet Server.dll
