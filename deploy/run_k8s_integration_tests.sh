podname="integration-test"

echo "Deploying pod with tests"
kubectl apply -f ./integration/mightycalc-api-tests.yaml
echo "Waiting container to start"
sleep 5
echo "Start logs streaming"
kubectl logs -f $podname | tee integration_tests.txt
#get pod running status. It can start sucessfully only if all of integration tests 
#ran fine 
result=$(\
    kubectl describe pods $podname| grep Ready: | head -1 | awk '{print $2}' | tr -d '\n')
if [ $result = "True" ]
then
    echo "tests passed"
    kubectl delete pod $podname
    exit 0
else
    echo "tests failed"
    exit 1
fi