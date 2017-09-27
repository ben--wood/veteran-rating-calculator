# Veteran Disability Rating Calculator

A simple API that returns a cumulative and estimated disability rating value according to the [Benefit Rates from the U.S. Department of Veterans Affairs website](https://www.benefits.va.gov/compensation/rates-index.asp).

### Do a GET or a POST to:
<a href="https://veteranratingcalculator.azurewebsites.net/api/calculate">https://veteranratingcalculator.azurewebsites.net/api/calculate</a>

### GET
Send comma or pipe separated rating values in the request:  
/api/calculate/40,20,60  

### POST
Send an array of integer rating values in the post body:  
/api/calculate  
Body: [40,20,60]  

### RETURNS
The API will return a JSON object that looks like this:  
`{"cumulativeDisabilityRating":81,"estimatedDisabilityRating":80}`

