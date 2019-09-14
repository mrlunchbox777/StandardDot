FROM microsoft/dotnet:2.2-sdk-alpine

ARG BASE_DIR

WORKDIR /app/${BASE_DIR}
EXPOSE 5000
CMD dotnet watch run