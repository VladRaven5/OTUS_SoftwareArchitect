helm install rabbit -f ./charts/rabbit/rabbitmq-values.yaml bitnami/rabbitmq
helm install mylists ./charts/lists-chart
helm install myprojmembers ./charts/project-members-chart
helm install myworkhours ./charts/working-hours-chart
helm install mytasks ./charts/tasks-chart