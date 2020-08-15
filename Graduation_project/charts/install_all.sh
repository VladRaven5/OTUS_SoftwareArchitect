helm install prometh stable/prometheus-operator -f ./prometheus/prometheus-values.yaml --atomic
helm install rabbit -f ./rabbit/rabbitmq-values.yaml bitnami/rabbitmq
helm install myauth auth-chart/
helm install myusers users-chart/
helm install mylabels labels-chart/
helm install mylists lists-chart/
helm install myprojmembers project-members-chart/
helm install myprojects projects-chart/
helm install mytasks tasks-chart/
helm install myworkhours working-hours-chart/
helm install myapi api-chart/
