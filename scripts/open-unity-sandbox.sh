#!/bin/bash

SCRIPT_DIR="$(dirname $(realpath $0))"
cd $SCRIPT_DIR

UNITY_PROJECT_DIR="$(realpath '../runtimes/Unity')"
UNITY_LOG_FILE="./Logs/Editor.log"
UNITY_DEV_SCRIPT_NAME="Aby.Unity.Editor.Actions.Build.Sandbox"
UNITY_DEV_SCRIPT_ARGS="-abyRuntimeStart"

while true;
do
    cd $UNITY_PROJECT_DIR
    echo "Opening Unity Editor with -executeMethod $UNITY_DEV_SCRIPT_NAME"
    Unity -projectPath $UNITY_PROJECT_DIR -executeMethod $UNITY_DEV_SCRIPT_NAME -logFile $UNITY_LOG_FILE -- $UNITY_DEV_SCRIPT_ARGS
    EXIT_CODE=$? # Capture Unity's exit code for restarting/error reporting.
    
    # TODO: Tail the project log(s).

    if [ $EXIT_CODE -eq 100 ];
    then
        echo "Unity requested restart."
        sleep 1
    elif [ $EXIT_CODE -eq 70 ];
    then
        echo "Deno force-quit ($EXIT_CODE) because of a failed feature verification."
        exit $EXIT_CODE
    elif [ $EXIT_CODE -ne 0 ];
    then
        echo "Unity failed with exit code $EXIT_CODE, exiting script."
        exit $EXIT_CODE
    else
        echo "Unity exited successfully."
        break
    fi
done

echo "Goodbye! <3"
exit 0
