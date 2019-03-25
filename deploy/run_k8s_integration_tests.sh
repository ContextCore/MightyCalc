podname="mightycalc-api-test"

echo "Deploying pod with tests"
kubectl apply -f ./integration/mightycalc-api-tests.yaml
echo "Waiting container to start"
sleep 2
echo "Start logs streaming"
kubectl logs -f $podname
result=$(\
    kubectl describe pods $podname| grep Ready: | head -1 | awk '{print $2}' | tr -d '\n')
#echo $result
if($result -eq "True")
then
    echo "tests passed"
    kubectl delete pod $podname
    exit 0
else
    echo "tests failed"
    exit 1
fi