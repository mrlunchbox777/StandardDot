# HMAC Help

[Home](/README.md)

## Boiler Plate Code

* [C#](/src/Authentication/Hmac/API%20Integration%Help/Integration%20Endpoints.md)
* [JS and Node](/src/Authentication/Hmac/API%20Integration%Help/C#.md)

## [Integration Endpoints](/src/Authentication/Hmac/API%20Integration%Help/Integration%20Endpoints.md)

## Basic Instructions

[A Basic README](/src/Authentication/Hmac/API%20Integration%Help/README.md)

We use HMAC Authentication to verify requests.

You will be given 2 values

1. AppId (public key)
2. Secret key (private key)

This HMAC implementation uses a single header to verify authenticity, content reliability, repeatability, time sync, and request delivery accuracy
When creating the various parts pay special attention to spacing.
Note the curly braces are just there to show the variables, DO NOT INCLUDE THEM.

The header will look like this

```http
[Authorization: sls AppId:Signature:Nonce:Timestamp]
```

The value more specifically like this

```http
sls {AppId}:{RequestSignatureBase64String}:{Nonce}:{RequestTimeStamp}
```

1. `:` - The first one is the key value seperator, the remaining `:`'s are seperator to get the different portions of the header value
2. `Authorization` - Header key
3. `sls` - Authentication namespace
4. `AppId` - The AppId you get from Social Wallet
5. `Signature` - A Base64 encoded HMAC SHA256 hash of the Signature Data (uses your secret key as the hashing key)
6. `Nonce` - An arbitrary string that you will generate to identify a unique request (Generate a new one for EVERY request)
7. `Timestamp` - Unix Timestamp (seconds since Jan 1st 1970) use UTC

The signature data will look like this

```http
{AppId}{RequestHttpMethod}{RequestUriString}{RequestTimeStamp}{Nonce}{RequestContentBase64String}
```

1. `AppId` - The AppId you get from the Provider
2. `RequestHttpMethod` - The Http Method used to call the Provider (All Caps, e.g. POST)
3. `RequestUriString` - The URI called (Include the whole thing, including all parameters)
4. `RequestTimeStamp` - Unix Timestamp (seconds since Jan 1st 1970) use UTC (must match the timestamp above)
5. `Nonce` - An arbitrary string that you will generate to identify a unique request (Generate a new one for EVERY request) (must match the nonce above)
6. `RequestContentBase64String` - A Base64 encoded MD5 hash of the request content

Steps in the process

1. Get your content, URI, and request method
2. Generate a nonce (guids work fine)
3. Generate a Unix Timestamp from UTC
4. MD5 hash your content, and then Base64 Encode the hash
5. Create your signature string
    - `{AppId}{RequestHttpMethod}{RequestUriString}{RequestTimeStamp}{Nonce}{RequestContentBase64String}`
6. HMAC SHA256 Hash your signature, using your Secret Key as the hashing key, and then Base64 Encode the hash - [C++](https://gist.github.com/woodja/6082940)
7. Create your header
    - Complete Header - `[Authorization: sls AppId:Signature:Nonce:Timestamp]`
    - Header Value - `sls {AppId}:{RequestSignatureBase64String}:{Nonce}:{RequestTimeStamp}`
8. Submit your header with your request, and you should be authenticated


There are several utility enpoints given in the [Integration Endpoints](/src/Authentication/Hmac/API%20Integration%Help/Integration%20Endpoints.md)