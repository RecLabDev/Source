#!/bin/bash

SCRIPT_DIR="$(dirname $(realpath $0))"
cd $SCRIPT_DIR

DOCS_DIR="$(realpath '../docs')"

mdbook serve $DOCS_DIR
