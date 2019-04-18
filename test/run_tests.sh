#set -euox pipefail
#set -x
mode=${1:-run} 
#mod - can be 'run', 'clean', or test name 

if [ $mode != "clean" ]
then
    echo "creating test pod" 
    kubectl apply -f mightycalc-tests.yaml
    ./wait_pod_is_ready.sh 180 3
fi

 kubectl exec test -- dotnet ef database update --project ./MightyCalc.Reports/MightyCalc.Reports.csproj --startup-project ./MightyCalc.Reports.IntegrationTests/MightyCalc.Reports.IntegrationTests.csproj

print_red()
{
    printf "\e[31m$1\e[m\n"
}
print_green()
{
     printf "\e[32m$1\e[m\n"
}

clean_all()
{
    rm -rf $1"_tests_output.txt"
    rm -rf $1"_Logs" 
    rm -rf $1"_test_logs.zip" 
}

runTests() {
    testsName=$1
    testFolder=$2
    podName=$3
    logsOutputFile=$testsName"_tests_output.txt"
    hostLogsDirName=$testsName"_Logs"
    hostLogsDir="./"$hostLogsDirName
    rm -rf $hostLogsDir
    mkdir $hostLogsDir

    testProject="./$testFolder/$testFolder.csproj"
    echo Running $testsName tests in $testProject


    kubectl exec $podName -- dotnet test $testProject -c Release --no-build --logger trx > $hostLogsDir/$logsOutputFile
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
    zip -r $name"_test_logs.zip"  $hostLogsDirName
    return $TestResult
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
testsPlan=(  "cfg:MightyCalc.Configuration.Tests"
             "api:MightyCalc.API.Tests"
             "node:MightyCalc.Node.Tests"
             "reports:MightyCalc.Reports.Tests"
             "node_integration:MightyCalc.Node.IntegrationTests"
             "report_integration:MightyCalc.Reports.IntegrationTests"
             "api_integration:MightyCalc.API.IntegrationTests")
anyTestFailed=0

for plan in "${testsPlan[@]}" ; do
    name="${plan%%:*}"
    project="${plan##*:}"
    #echo processing $name
    if [ $mode == "clean" ]
    then
        clean_all $name 
    elif [ $mode == "run" ] || [ $mode == $name ] 
    then
       launchTest $name $project
       kubectl exec seed-0 -- pbm localhost:9110 cluster show
    fi;
done

if [ $mode == "clean" ]
then
  echo cleaning previos test run results
  kubectl delete pod test --wait=false
fi
#exit $anyTestFailed 
