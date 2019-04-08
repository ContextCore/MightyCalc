set -euo pipefail
# this script will create an environment on target Kubernetes cluster
k8sProvider=$1 #gke or dd (docker-desktop) or mk (minikube)


#-----------##Persistence##----------
kubectl apply -f ./persistence/storageclass-${k8sProvider}.yaml
#kubectl apply -f ./persistence/persistence-volumeclaim.yaml
kubectl apply -f ./persistence/persistence-deploy.yaml
kubectl apply -f ./persistence/persistence-svc.yaml


if [ $k8sProvider = "dd" ]
then
kubectl apply -f ./persistence/persistence-svc-external.yaml
fi

if [ $k8sProvider = "dd" ] || [ $k8sProvider = "mk" ]
then
kubectl apply -f ./api/mightycalc-api-svc-external-dd.yaml
else 
kubectl apply -f ./api/mightycalc-api-svc-external-gke.yaml
fi

#-----------##Nodes##----------
kubectl apply -f ./node/seed-svc.yaml 
kubectl apply -f ./node/seed.yaml
kubectl apply -f ./node/worker.yaml
#-----------##API##----------
kubectl apply -f ./api/mightycalc-api-deploy.yaml
kubectl apply -f ./api/mightycalc-api-svc.yaml
