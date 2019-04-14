set -euo pipefail
# this script will create an environment on target Kubernetes cluster
k8sProvider=$1 #gke or dd (docker-desktop) or mk (minikube)


#-----------##Persistence##----------
kubectl apply -f ./persistence/storageclass-${k8sProvider}.yaml

if [ $k8sProvider = "mk" ]
then
kubectl apply -f ./persistence/postgres-mk.yaml
else
kubectl apply -f ./persistence/postgres.yaml
fi

kubectl apply -f ./persistence/postgres-svc.yaml
kubectl apply -f ./persistence/postgres-svc-external.yaml

if [ $k8sProvider = "dd" ] || [ $k8sProvider = "mk" ]
then
kubectl apply -f ./api/api-svc-external-dd.yaml
else 
kubectl apply -f ./api/api-svc-external-gke.yaml
fi

#-----------##Nodes##----------
kubectl apply -f ./node/seed-svc.yaml 
kubectl apply -f ./node/seed.yaml
kubectl apply -f ./node/worker.yaml
#-----------##API##----------
kubectl apply -f ./api/api.yaml
kubectl apply -f ./api/api-svc.yaml
