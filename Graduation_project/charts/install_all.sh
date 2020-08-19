echo -e "Starting nginx\n"
helm install nginx stable/nginx-ingress -f ./nginx/values.yaml --atomic
echo -e "\n\nStarting prometheus\n"
helm install prometh stable/prometheus-operator -f ./prometheus/prometheus-values.yaml --atomic
echo -e "\n\nStarting RabbitMQ\n"
helm install rabbit -f ./rabbit/rabbitmq-values.yaml bitnami/rabbitmq
echo -e "\n\nStarting Users shards\n"
helm install cn-users -f ./users-shards-values/cn-values.yaml users-shards-chart/
helm install eu-users -f ./users-shards-values/eu-values.yaml users-shards-chart/
helm install ru-users -f ./users-shards-values/ru-values.yaml users-shards-chart/
helm install us-users -f ./users-shards-values/us-values.yaml users-shards-chart/
echo -e "\n\nStarting Auth service\n"
helm install myauth auth-chart/
echo -e "\n\nStarting Users service\n"
helm install myusers users-chart/
echo -e "\n\nStarting Labels service\n"
helm install mylabels labels-chart/
echo -e "\n\nStarting Lists service\n"
helm install mylists lists-chart/
echo -e "\n\nStarting Project members service\n"
helm install myprojmembers project-members-chart/
echo -e "\n\nStarting Projects service\n"
helm install myprojects projects-chart/
echo -e "\n\nStarting Tasks service\n"
helm install mytasks tasks-chart/
echo -e "\n\nStarting Working hours service\n"
helm install myworkhours working-hours-chart/
echo -e "\n\nStarting Notifications service\n"
helm install mynotif notifications-chart/
echo -e "\n\nStarting Api service\n"
helm install myapi api-chart/
echo -e "\n\nDone\n"
