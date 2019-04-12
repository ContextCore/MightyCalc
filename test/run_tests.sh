#set -euox pipefail
#set -x
mode=${1:-run} 
if [ $mode != "clean" ]
then
    echo "creating test pod" 
    kubectl apply -f mightycalc-tests.yaml
    ./wait_pod_is_ready.sh 60 1
else
  echo cleaning previos test run results
  kubectl delete pod test
fi

runTests() {
    testsName=$1
    testFolder=$2
    podName=$3
    logsOutputFile=${testsName}_tests_output.txt 
    testProject="./$testFolder/$testFolder.csproj"
    echo Running $testsName tests in $testProject

    hostLogsDirName=$testsName"_Logs"
    hostLogsDir="./"$hostLogsDirName
    rm -rf $hostLogsDir
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
    #rm -rf $hostLogsDir
    return $TestResult
}

print_red()
{
    printf "\e[31m$1\e[m\n"
}
print_green()
{
     printf "\e[32m$1\e[m\n"
}
launchTest()
{
    name=$1
    project=$2

     runTests $name $project test
    
        testExitCode=$?
        if [ $testExitCode -ne 0 ]
        then 
            print_red "$name tests in $project failed, see logs for details"
     # exit $testExitCode
        else
            print_green "$project tests passed" 
        fi
}
testsPlan=( "api:MightyCalc.API.Tests"
             "node:MightyCalc.Node.Tests"
             "reports:MightyCalc.Reports.Tests"
             "node_integration:MightyCalc.Node.IntegrationTests"
             "report_integration:MightyCalc.Reports.IntegrationTests"
             "api_integration:MightyCalc.API.IntegrationTests")
anyTestFailed=0

for plan in "${testsPlan[@]}" ; do
    name="${plan%%:*}"
    project="${plan##*:}"
    
    if [ $mode == "clean" ]
    then
        rm -rf $name"_tests_output.txt" 
        rm -rf $name"_Logs" 
        rm -rf $name"_test_logs.zip" 
    else 
       launchTest $name $project
    fi
    
done

#exit $anyTestFailed 
