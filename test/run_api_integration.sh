. ./get_pod_name.sh

apiPodName=$(get_pod api)
workerPodName=$(get_pod worker)

kubectl exec seed-0  -- pbm localhost:9110 cluster tail > seed_cluster_events.txt &
kubectl logs -f seed-0  > seed_pod_logs.txt &

kubectl logs -f postgres-0  > postgres_pod_logs.txt &

kubectl exec $workerPodName -- pbm localhost:9110 cluster tail > worker_cluster_events.txt &
kubectl logs -f $workerPodName  > worker_pod_logs.txt &

kubectl exec $apiPodName -- pbm localhost:9110 cluster tail > api_cluster_events.txt &
kubectl logs -f $apiPodName  > api_pod_logs.txt &

#./run_tests.sh api_integration

./run_tests 

echo describing services
#kubectl describe service postgres-svc 
#kubectl describe pod postgres-0

zip -r "seed_cluster_events_logs.zip"  ./seed_cluster_events.txt
zip -r "worker_cluster_events_logs.zip"  ./worker_cluster_events.txt
zip -r "api_cluster_events_logs.zip"  ./api_cluster_events.txt
zip -r "seed_pod_logs.zip"  ./seed_pod_logs.txt
zip -r "worker_pod_logs.zip"  ./worker_pod_logs.txt
zip -r "api_pod_logs.zip"  ./api_pod_logs.txt
zip -r "postgres_pod_logs.zip"  ./postgres_pod_logs.txt
