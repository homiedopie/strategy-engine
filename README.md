# Strategy Engine

This is an initial implementation of the strategy engine on searching with 4 filters (AND condition) and returning the right output value.

## Instructions

Prerequisites:
- Docker

Steps:
1. Run `docker build -t strategy-engine-test -f Tests.Dockerfile .` to create the test image.
2. Run `docker run strategy-engine-test` to verify if all test are passing
3. Run `docker build -t strategy-engine -f Dockerfile .` to build the app image
4. Run `docker run strategy-engine` to verify output of the results
5. Run `docker run --rm -v "$(pwd)/TestData:/App/TestData" -it strategy-engine /App/TestData/SampleData.csv /App/TestData/ResultData.csv` for custom input/result