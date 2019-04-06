#set -euox pipefail
#set -x
echo "creating test pod" 
kubectl apply -f mightycalc-tests.yaml
./wait_pod_is_ready.sh 60 1

runTests() {
    testsName=$1
    testFolder=$2
    podName=$3
    logsOutputFile=${testsName}_tests_output.txt 
    testProject="./$testFolder/$testFolder.csproj"
    echo Running $testsName tests in $testProject

    hostLogsDirName="Logs"
    hostLogsDir="./"$hostLogsDirName
    mkdir $hostLogsDir

    kubectl exec -it $podName -- dotnet test $testProject -c Release --no-build --logger trx > $hostLogsDir/$logsOutputFile
    TestResult=$?

    projectFolder=/usr/bin/MightyCalc/${testFolder}
    podTestResultFolder=$projectFolder/Logs

    echo searching for log files
    kubectl exec $podName -- mkdir $podTestResultFolder
    kubectl exec $podName -- find $projectFolder/bin -name '*.txt' -exec cp {} $podTestResultFolder \;
    kubectl exec $podName -- find $projectFolder/bin -name '*.log' -exec cp {} $podTestResultFolder \;
    kubectl exec $podName -- find $projectFolder/TestResults -name '*.trx' -exec cp {} $podTestResultFolder \;
    kubectl cp $podName:$podTestResultFolder $hostLogsDir
    
    #echo Looking for a tests results. It may be the direct output and any additional logs in *.txt files produced by 
    #test runner
    zip -r ${testsName}_test_logs.zip $hostLogsDirName
    rm -rf $hostLogsDir
    return $TestResult
}

testsPlan=( "api:MightyCalc.API.Tests"
             "node:MightyCalc.Node.Tests"
             "reports:MightyCalc.Reports.Tests"
             "api_integration:MightyCalc.Node.IntegrationTests"
             "report_integration:MightyCalc.Reports.IntegrationTests"
             "node_integration:MightyCalc.Node.IntegrationTests")
anyTestFailed=0

for plan in "${testsPlan[@]}" ; do
    name="${plan%%:*}"
    project="${plan##*:}"
    
    runTests $name $project mightycalc-test

    testExitCode=$?
    if [ $testExitCode -ne 0 ]
    then 
      echo $name tests in $project failed, see logs for details
      exit $testExitCode
    fi
done

exit $anyTestFailed 