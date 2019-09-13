# Integration Endpoints

## Navigation

* [Home](/README.md)
	* [Index](/docs/Index.md)
		* [Authentication](/src/Authentication/README.md)
			* [Hmac](/src/Authentication/Hmac/README.md)
				* [Hmac Authentication Help](/src/Authentication/Hmac/API%20Integration%20Help/HMAC%20Authentication%20Help.md)

### Children

## Info

### Table of Contents

### Basic Methods

#### Anonymous

1. Heartbeat, GET `https://{host}/api/{controller}`
2. Verify App Id Is Valid And Usable, GET `https://{host}/api/{controller}/VerifyAppIdIsValidAndUsable?appId={appId}`
	* We submit the request to ourselves for the given app id and send back the response
3. Verify Proper Rejection, GET `https://{host}/api/{controller}/VerifyProperRejection`
	* We submit a bad request to ourselves and send back the response
4. Verify App Id Is Valid And Usable With Content, GET `https://{host}/api/{controller}/VerifyAppIdIsValidAndUsableWithContent?appId={appId}`
	* We submit the request to ourselves for the given app id and send back the response
5. Verify Proper Rejection With Content, GET `https://{host}/api/{controller}/VerifyProperRejectionWithContent`
6. Get Basic Content To Post, GET `https://{host}/api/{controller}/GetBasicContentToPost`
	* Example

```json
{
	"Name": "BasicContent",
	"Id": "2f097177-a9aa-4851-a4b3-403c7e05bff6"
}

Types

{
	"Name": "string",
	"Id": "string"
}
```

#### Authenticated

1. Verify Authentication, GET `https://{host}/api/{controller}/VerifyAuthentication`
2. Verify Application With Content, POST `https://{host}/api/{controller}/VerifyAuthenticationWithContent`
	* Post the object from Get Basic Content To Post, any values of the same type

### PROVIDER RELATED METHODS

#### Provider Authenticated

1. Fill this in

#### Provider Anonymous

1. Fill this in