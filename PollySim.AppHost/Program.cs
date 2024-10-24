var builder = DistributedApplication.CreateBuilder(args);

var grafana = builder.AddContainer("grafana", "grafana/grafana")
                     .WithBindMount("../grafana/config", "/etc/grafana", isReadOnly: true)
                     .WithBindMount("../grafana/dashboards", "/var/lib/grafana/dashboards", isReadOnly: true)
                     .WithHttpEndpoint(targetPort: 3000, name: "http");

builder.AddContainer("prometheus", "prom/prometheus")
       .WithBindMount("../prometheus", "/etc/prometheus", isReadOnly: true)
       .WithHttpEndpoint(/* This port is fixed as it's referenced from the Grafana config */ port: 9090, targetPort: 9090);

builder.AddProject<Projects.PollySim_Runner>("PollySimRunner");

using var app = builder.Build();

await app.RunAsync();
