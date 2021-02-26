# powerplant-coding-challenge

## Requirements

This project use .NET 5.0 as framework. Make sure you have the [SDK](https://dotnet.microsoft.com/download/dotnet/5.0) installed.

## Build and test the app

### In Visual Studio or Rider

Restore and build the app like any project. To run the unit tests, use the unit tests window.

### In CLI

In the src folder, execute the following commands:

- `dotnet restore`
- `dotnet build`
- `dotnet test`

## Launch the app

### In Visual Studio or Rider

Launch the `PowerPlantChallenge.WebApi` or `IIS Express` profile. The REST API is served on http://localhost:8888 and a swagger is available at http://localhost:8888/swagger/index.html when the app is launched in the development environnement.

### In CLI

Go to the subfolder `src/PowerPlantChallenge.WebApi` and execute `dotnet run`.

## Docker

A Dockerfile is available in the root folder. It will build, test and publish the app. If a unit test failed during the process, it stops so we can be sure there is never an invalid image available in the registry. 

In the src folder, execute the following commands:

- `docker build -t power-plant-challenge:latest .`
- `docker run --rm -it -p 8888:80 power-plant-challenge:latest`

## Continuous Integration

A github action workflow is run for each commit. It builds and tests the app to be sure that all commits are always valid.

## Quick explanation of the algorithm 

1. From powerplants of the production plan, create their respective powerplantloads. These last contain the real minimum and maximum power of the powerplant, they are only different when the powerplant is a wind turbine and the wind percentage is different from 100%. In addition, the powerplantloads also have a prop to hold the power that the powerplant need to supply to respect the needed load of the production plan.


2. Powerplantloads are sorted by the production price of one MWh of electricity based on the fuels price (0 in the case of a wind turbine), the efficiency and, only for the gas-fired, the cost in Co2. If two powerplant have the same price, the one that produces the most is used first.


3. Check if there is at least one powerplant which has a minimum power less than or equal to the requested load. Otherwise, throw an exception.


4. Check if the maximum real power of all power plants is greater than or equal to the requested load. Otherwise, throw an exception.


5. For each powerplant, find the best output power to respect the requested load.
    1. If the Pmin <= remaining load to find <= Pmax, use it as output power (remaining load is now 0)
    2. If the remaining load > Pmax, use the Pmax as output power (Pmax is subtract of remaining load)
    3. If the remaining load < Pmin, calculate the extra power produced by using the Pmin of this powerplant.
       1. Check if we can remove this extra power from the previous powerplants.
       2. If not, do not use this powerplant for the plan. (remaining load is the same)
       3. If we can, adapt the powers of previous powerplants to remove this extra power and use the Pmin as power for this plant. (remaining load is now 0)


6. Check if the remaining load to plan is equal to 0, return the powerplantloads. Otherwise throw an exception: “no combination of powerplants can find the requested load”.

*Original Readme*

## Welcome !

Below you can find the description of a coding challenge that we ask people to perform when applying
for a job in our team.

The goal of this coding challenge is to provide the applicant some insight into the business we're in and
as such provide the applicant an indication about the challenges she/he will be confronted with. Next, during the
first interview we will use the applicant's implementation as a seed to discuss all kinds of 
interesting software engineering topics.  

## Who are we 

We are the IS team of the 'Short-term Power as-a-Service' (a.k.a. SPaaS) team within [GEM](https://gems.engie.com/).

[GEM](https://gems.engie.com/), which stands for 'Global Energy Management', is the 
energy management arm of [ENGIE](https://www.engie.com/), one of the largest global energy players, 
with access to local markets all over the world.  

SPaaS is a team consisting of around 100 people with experience in energy markets, IT and
modeling. In smaller teams consisting of a mix of people with different experiences, we are active on
the [day-ahead](https://en.wikipedia.org/wiki/European_Power_Exchange#Day-ahead_markets) market, 
[intraday markets](https://en.wikipedia.org/wiki/European_Power_Exchange#Intraday_markets) and
[collaborate with the TSO to balance the grid continuously](https://en.wikipedia.org/wiki/Transmission_system_operator#Electricity_market_operations).


## The challenge

### In short
Calculate how much power each of a multitude of different [powerplants](https://en.wikipedia.org/wiki/Power_station) need 
to produce (a.k.a. the production-plan) when the [load](https://en.wikipedia.org/wiki/Load_profile) is given
and taking into account the cost of the underlying energy sources (gas,  kerosine) and the Pmin and Pmax of each powerplant.

### More in detail

The load is the continuous demand of power. The total load at each moment in time is forecasted. For instance
for Belgium you can see the load forecasted by the grid operator [here](https://www.elia.be/en/grid-data/load-and-load-forecasts).

At any moment in time, all available powerplants need to generate the power to exactly match the load.
The cost of generating power can be different for every powerplant and is dependent on external factors:
The cost of producing power using a [turbojet](https://en.wikipedia.org/wiki/Gas_turbine#Industrial_gas_turbines_for_power_generation), 
that runs on kerosine, is higher compared to the cost of generating power 
using a gas-fired powerplant because of gas being cheaper compared to kerosine and because of the 
[thermal efficiency](https://en.wikipedia.org/wiki/Thermal_efficiency) of a gas-fired powerplant being around
50% (2 units of gas will generate 1 unit of electricity) while that of a turbojet is only around 30%.
The cost of generating power using windmills however is zero. Thus deciding which powerplants to
activate is dependent on the [merit-order](https://en.wikipedia.org/wiki/Merit_order).

When deciding which powerplants in the merit-order to activate 
(a.k.a. [unit-commitment problem](https://en.wikipedia.org/wiki/Unit_commitment_problem_in_electrical_power_production)) 
the maximum amount of power each powerplant can produce (Pmax) obviously needs to be taken into account. 
Additionally gas-fired powerplants generate a certain minimum amount of power when switched on, called the Pmin. 


### Performing the challenge

Build a REST API exposing an endpoint `/productionplan` that accepts a POST with a payload as you can 
find in the `example_payloads` directory and that returns a json with the same structure as 
in `example_response.json` and that manages and logs run-time errors.

For calculating the unit-commitment, we prefer you not to rely on an existing (linear-programming) solver but
instead write an algorithm yourself.

Implementations can be coded in either in C#, Go or Python as these are (currently) the main languages we use in SPaaS.
Along with the implementation should be a README that describes how to compile (if applicable) and launch the application.

- C# implementations should contain a solutions file to compile the application. 
- Python implementations should contain
a `requirements.txt` or a `pyproject.toml` (for use with poetry) to install all needed dependencies.

#### Payload

The payload contains 3 types of data:
 - load: The load is the amount of energy (MWh) that need to be generated during one hour.
 - fuels: based on the cost of the fuels of each powerplant, the merit-order can be determined which is the starting
 point for deciding which powerplants should be switched on and how much power they will deliver.
 Wind-turbine are either switched-on, and in that case generate a certain amount of energy 
 depending on the % of wind, or can be switched off. 
   - gas(euro/MWh): the price of gas per MWh. Thus if gas is at 6 euro/MWh and if the efficiency of the powerplant is 50%
   (i.e. 2 units of gas will generate one unit of electricity), the cost of generating 1 MWh is 12 euro.
   - kerosine(euro/Mwh): the price of kerosine per MWh.
   - co2(euro/ton): the price of emission allowances (optionally to be taken into account).
   - wind(%): percentage of wind. Example: if there is on average 25% wind during an hour, a wind-turbine 
   with a Pmax of 4 MW will generate 1MWh of energy.
 - powerplants: describes the powerplants at disposal to generate the demanded load. For each powerplant.
 is specified:
   - name:
   - type: gasfired, turbojet or windturbine.
   - efficiency: the efficiency at which they convert a MWh of fuel into a MWh of electrical energy.
   Wind-turbines do not consume 'fuel' and thus are considered to generate power at zero price.
   - pmax: the maximum amount of power the powerplant can generate.
   - pmin: the minimum amount of power the powerplant generates when switched on. 

#### response

The response should be a json as in `example_response.json`, specifying for each powerplant how much 
power each powerplant should deliver. The power produced by each powerplant has to be a multiple
of 0.1 Mw and the sum of the power produced by all the powerplants together should
equal the load. 

### Want more challenge?

Having fun with this challenge and want to make it more realistic. Optionally, do one of the extra's below:

#### Docker

Provide a Dockerfile along with the implementation to allow deploying your solution quickly.

#### CO2

Taken into account that a gas-fired powerplant also emits CO2, the cost of running the powerplant should
also take into account the cost of the [emission allowances](https://en.wikipedia.org/wiki/Carbon_emission_trading).
For this challenge, you may take into account that each MWh generated creates 0.3 ton of CO2. 

#### Websocket

Provide a websocket server connection that will emit after every post the input of the POST together with
the response to every client connected on the websocket.

## Acceptance criteria

For a submission to be reviewed as part of an application for a position in the team, the project
needs to:
  - contain a README.md explaining how to build and launch the API
  - expose the API on port `8888`
  - return a result where the sum of the power generated by each of the different powerplants is
  exactly equal to the load specified in the payload for at least the example payloads provided.

Failing to comply with any of these criteria will automatically disqualify the submission.

## More info

For more info on energy management, check out:

 - [Global Energy Management Solutions](https://www.youtube.com/watch?v=SAop0RSGdHM)
 - [COO hydroelectric power station](https://www.youtube.com/watch?v=edamsBppnlg)
 - [Management of supply](https://www.youtube.com/watch?v=eh6IIQeeX3c) - video made during winter 2018-2019

## FAQ

##### Can an existing solver be used to calculate the unit-commitment
Implementations should not rely on an external solver and thus contain an algorithm written
from scratch (clarified in the text as of version v1.1.0)
