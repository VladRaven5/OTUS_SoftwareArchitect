helm install nginx stable/nginx-ingress -f ./charts/nginx/values.yaml --atomic
helm install prometh stable/prometheus-operator -f ./charts/prometheus/prometheus-values.yaml --atomic
helm install mytasks ./charts/tasks-chart