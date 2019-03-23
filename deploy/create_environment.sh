# this script will create an environment on target Kubernetes cluster
k8sProvider=$1 #gke or dd (docker-desktop)

kubectl apply -f mightycalc-api-deploy.yaml
kubectl apply -f mightycalc-api-svc.yaml
kubectl apply -f mightycalc-api-svc-external.yaml

kubectl apply -f persistence/persistence-deploy.yaml
kubectl apply -f persistence/persistence-svc.yaml
kubectl apply -f persistence/persistence-svc-external.yaml
kubectl apply -f persistence/storageclass-${k8sProvider}.yaml
