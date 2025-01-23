## Run Locally

### Install Docker CLI

The easiest way to run the server locally is by using docker CLI.
Make sure it's installed by running the following command on you terminal

```
docker -v
```

it should read something like

```
Docker version 24.0.2, build cb74dfc
```

### Build the image

Position yorself at the root of the project and run the following command

```
 docker build  -f "./PokedexProject/Dockerfile" -t 'pokedex_api' .
```

Once completed, run the following command to run a container using the newly created image

```
 docker run --detach -p 5000:8080 'pokedex_api'
```

this binds you local port 5000 to the exposed 8080 port from the container

Now you should be able to query the server by running (on Windows)

```
powershell -command "Invoke-WebRequest -Uri http://localhost:5000/pokemon/Charizard -Method GET"
```

and receive the content

```
{
    "description": "Spits fire that is hot enough to melt boulders. Known to cause forest fires unintentionally.",
    "habitat": "mountain",
    "is_legendary": false,
    "name": "Charizard"
}
```

## Deployment

The server is deployed at the following address:

https://pokedexproject-jebl.onrender.com

Please be aware that the response time might be very long (minutes if the container needs to be spun up) since i'm using a free tier subscription.

## Considerations on production

Although a simple service, there are some considerations that can be make whether this were to go on production

- if this endpoint where to be scaled, there could be come performance advantages by using a shared remote cache using Redis or similar services and controlling the behaviour with some env parameter
- if this service where to run in a distributed environment as a mircoservice, many different aspects such as rate-limiting, resilience, circuit-break policy etc. need to be considered. Some of these concerns may be alleviate by using a library like Polly
- regarding language, we should consider all the localization complications since the pokeapi endpoint can support many different languages. This could controlled using, for example, the accept-language header coming with the request

## Final Consideration

One final note that i want to make is that i interpreted the statement "**_The API response should contain a minimum of:_**" as that the four attributes specified MUST be populated, and return an error otherwise. In fact, searching the pokemon "Dialga" which has a null value for the Habitat, return a NotFound error specifying that the habitat is not present.
