kubectl apply \
-f ./manifests/configmap.yaml \
-f ./manifests/postgres-ss.yaml \
-f ./manifests/postgres-serv.yaml \
-f ./manifests/postgres-migration-job.yaml \
-f ./manifests/deployment.yaml \
-f ./manifests/service.yaml \
-f ./manifests/ingress.yaml
