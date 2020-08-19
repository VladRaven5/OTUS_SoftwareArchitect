echo "\nStarting Users shards\n"
helm install cn-users -f ./charts/users-shards-values/cn-values.yaml ./charts/users-shards-chart/
helm install eu-users -f ./charts/users-shards-values/eu-values.yaml ./charts/users-shards-chart/
helm install ru-users -f ./charts/users-shards-values/ru-values.yaml ./charts/users-shards-chart/
helm install us-users -f ./charts/users-shards-values/us-values.yaml ./charts/users-shards-chart/
echo "\n\nStarting Users service\n"
helm install myusers ./charts/users-chart/