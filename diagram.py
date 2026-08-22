from diagrams import Diagram, Cluster, Edge
from diagrams.azure.compute import FunctionApps
from diagrams.azure.database import CosmosDb
from diagrams.azure.web import StaticApps
from diagrams.azure.identity import ManagedIdentities
from diagrams.azure.monitor import ApplicationInsights, LogAnalyticsWorkspaces, AzureWorkbooks, Monitor
from diagrams.azure.general import Subscriptions
from diagrams.onprem.client import Users

graph_attr = {
    "fontsize": "20",
    "bgcolor": "white",
    "pad": "0.6",
    "splines": "ortho",
}

with Diagram(
    "Azure FinOps Cost Visibility Dashboard",
    filename="docs/architecture",
    outformat="png",
    graph_attr=graph_attr,
    show=False,
):
    user = Users("Browser")

    with Cluster("Azure Subscription  ·  rg-finops-dev  ·  East US"):

        swa = StaticApps("Static Web Apps\nReact + Recharts")

        with Cluster("Azure Functions  ·  .NET 8  ·  Consumption"):
            http_fn = FunctionApps("HTTP Triggers\n/costs/daily · /costs/by-resource\n/costs/by-tag · /optimization/waste\n/tags/hygiene · /anomalies · /forecasts")
            timer_fn = FunctionApps("Timer Triggers\nCostIngestion 06:00\nAnomalyDetection 06:30\nForecast 07:00 UTC")

        with Cluster("Cosmos DB  ·  Free Tier  ·  RBAC-only"):
            cosmos = CosmosDb("SQL API\ndaily-costs\nanomalies\nforecasts\nbudgets")

        with Cluster("Governance"):
            policy = Subscriptions("Azure Policy\nAudit: require cost_center tag")

        with Cluster("Observability"):
            ai = ApplicationInsights("Application Insights")
            la = LogAnalyticsWorkspaces("Log Analytics\n30-day retention")

        budget = Monitor("Azure Monitor\nBudget Alerts\n80% forecast\n100% actual")
        mi = ManagedIdentities("Managed Identity\nCost Mgmt Reader\nReader (Advisor + tags)\nCosmos DB Contributor")

    cost_api = AzureWorkbooks("Cost Management + Advisor API\nmanagement.azure.com")

    user >> swa >> http_fn
    http_fn >> Edge(label="read") >> cosmos
    http_fn >> Edge(label="cost-by-tag · advisor") >> cost_api
    timer_fn >> Edge(label="query costs") >> cost_api
    timer_fn >> Edge(label="upsert") >> cosmos
    policy >> Edge(label="audit resources", style="dashed") >> cosmos
    http_fn >> Edge(label="telemetry", style="dashed") >> ai
    timer_fn >> Edge(label="telemetry", style="dashed") >> ai
    ai >> la
    budget >> Edge(label="email alert", style="dashed", color="red") >> mi
    mi >> Edge(label="RBAC", style="dashed") >> http_fn
    mi >> Edge(label="RBAC", style="dashed") >> timer_fn
