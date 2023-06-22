FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy AS final

RUN apt-get update \
    && apt-get upgrade -y \
    && apt-get install -y \
        telnet \
		curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /publish

EXPOSE 5023

COPY KestrelTest/bin/Release/net7.0/publish .

ENV ASPNETCORE_URLS="http://+:5023"
ENTRYPOINT ["dotnet", "KestrelTest.dll"]
