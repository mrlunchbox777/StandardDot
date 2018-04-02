# Javascript and NodeJS Boiler Plate Code


[Other libraries](https://www.jokecamp.com/blog/examples-of-creating-base64-hashes-using-hmac-sha256-in-different-languages/)

Switch var to const if desired

You will need to provide a nonce method `nonce()`, the httpmethod `requestHttpMethod`, the URL (must be url encoded) `urlEncodedRequestUriString`, `APP_ID`, and `SECRET_KEY`

This is nodejs but can be replicated in regular js fairly easily

[nodejs library](https://nodejs.org/api/crypto.html)

[regular javascript library](https://www.npmjs.com/package/crypto-js) -- this can be used in non node projects (see without RequireJS). You'll have to replace the hashing calls below.

```javascript
var nonceString = nonce();
// now gets UTC
var ts = Math.round((Date.now()).getTime() / 1000);

var base64Content = crypto.createHash('md5').update(JSON.stringify(data)).digest('base64');
// regular js
// var contentBytes  = CryptoJS.MD5.(JSON.stringify(data));
// var base64Content = contentBytes.toString(CryptoJS.enc.Base64);

var signature = `${APP_ID}${requestHttpMethod}${urlEncodedRequestUriString}${ts}${nonceString}${base64Content}`;

// if the secret key is kept as a base64 encoded string representing a byte[] (or buffer) you'll need this
// and when you use the buffer pass the buffer as the key in the hmac hashing
// var buf = Buffer.from(SECRET_KEY, 'base64');
var signatureHash = crypto.createHmac('sha256', SECRET_KEY).update(signature).digest('base64');
// regular js
// var signatureBytes  = CryptoJS.HmacSHA256(signature, SECRET_KEY);
// var signatureHash = signatureBytes.toString(CryptoJS.enc.Base64);

var authString = `sls ${APP_ID}:${signatureHash}:${nonceString}:${ts}`;
```

Then add the `authString` to the headers of the request.