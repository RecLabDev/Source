#!/bin/bash

SCRIPT_DIR="$(dirname $(realpath $0))"
cd $SCRIPT_DIR

UNITY_PROJECT_DIR="$(realpath '../runtimes/Unity')"
UNITY_DEV_SCRIPT_NAME="Theta.Unity.Editor.Actions.Build.Sandbox"

while true;
do
    cd $UNITY_PROJECT_DIR
    echo "Opening Unity Runtime (with Dev Script \`$UNITY_DEV_SCRIPT_NAME\`)"
    Unity -projectPath $UNITY_PROJECT_DIR -executeMethod $UNITY_DEV_SCRIPT_NAME -logFile "./Logs/Project.log"
    EXIT_CODE=$? # Capture Unity's exit code for restarting/error reporting.
    
    # TODO: Tail the project log(s).

    if [ $EXIT_CODE -eq 100 ];
    then # Unity wants to restart.
        echo "Unity requested restart."
        sleep 1
    elif [ $EXIT_CODE -ne 0 ];
    then # Unity failed with exit code #.
        echo "Unity failed, exiting script."
        exit 1
    else
        # If Unity exits with 0, assume successful completion and exit the loop
        echo "Unity exited successfully."
        break
    fi
done

echo "Goodbye! <3"