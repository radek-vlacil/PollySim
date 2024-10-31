# PollySim
Simple simulation of Polly pipelines

## Installation
You need to install Docker and then just build and run the project.

## Usage
Three services are started when the AppHost Aspire project is run:
- Prometheus - service for collecting the metrics
- Grafana - service for displaying the telemetry via pre-created dashboards
- Runner - service which runs the experiments

Currently all the ports are fixed:
- Grafana: 3001
- Prometheus: 9090
- Runner: 5190

If you have any of these ports occupied you will need to reconfigure the following:
- Grafana 3001:
    - Change the port in the AppHost [here](https://github.com/radek-vlacil/PollySim/blob/6e0929aba10fa042df9e5852f7f0fa4616a328c1/PollySim.AppHost/Program.cs#L6C40-L6C49)
- Prometheus 9090:
    - Change the port in the AppHost [here](https://github.com/radek-vlacil/PollySim/blob/6e0929aba10fa042df9e5852f7f0fa4616a328c1/PollySim.AppHost/Program.cs#L10)
    - Change the port in the Grafana config [here](https://github.com/radek-vlacil/PollySim/blob/6e0929aba10fa042df9e5852f7f0fa4616a328c1/grafana/config/provisioning/datasources/default.yaml#L8)
- Runner 5190:
    - Change the port in the Runner [launch settings](Properties/launchSettings.json)
    - Change the port in the Prometheus config [here](https://github.com/radek-vlacil/PollySim/blob/6e0929aba10fa042df9e5852f7f0fa4616a328c1/prometheus/prometheus.yml#L7)
 
### Runner
Hit the service with a GET request and it will show the list of paths which are supported by the Runner. Currently it is ['/retry', '/timeout', '/cb', '/hedging'].
You can start the desired experiment by sending the GET request to the corresponding path.

### Grafana
Grafana is deployed with the pre-created dashboards. There are currently two of them:
- Polly - used to show the retry, timeout and circuit breaker
- Hedging - used to show the hedging

You can update the dashboards anyway you like. If you need to store it for the consecutive runs, just export the dashboard and store it into the grafana dashboard [folder](grafana/dashboards).

### Aspire portal
You can of course use also the Aspire default portal to look into logs and traces.
