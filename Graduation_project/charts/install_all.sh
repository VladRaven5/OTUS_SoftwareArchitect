echo "Starting prometheus\n"
helm install prometh stable/prometheus-operator -f ./prometheus/prometheus-values.yaml --atomic
echo "\n\nStarting RabbitMQ\n"
helm install rabbit -f ./rabbit/rabbitmq-values.yaml bitnami/rabbitmq
echo "\n\nStarting Auth service\n"
helm install myauth auth-chart/
echo "\n\nStarting Users service\n"
helm install myusers users-chart/
echo "\n\nStarting Labels service\n"
helm install mylabels labels-chart/
echo "\n\nStarting Lists service\n"
helm install mylists lists-chart/
echo "\n\nStarting Project members service\n"
helm install myprojmembers project-members-chart/
echo "\n\nStarting Projects service\n"
helm install myprojects projects-chart/
echo "\n\nStarting Tasks service\n"
helm install mytasks tasks-chart/
echo "\n\nStarting Working hours service\n"
helm install myworkhours working-hours-chart/
echo "\n\nStarting Notifications service\n"
helm install mynotif notifications-chart/
echo "\n\nStarting Api service\n"
helm install myapi api-chart/
echo "\n\nDone\n"
