#!/bin/sh
mkdir -p ./XPathTest/assets/XQUTS ./XPathTest/assets/QT3TS
curl -L https://github.com/LeoWoerteler/QT3TS/archive/master.tar.gz | tar -xz -C ./XPathTest/assets/QT3TS --strip-components=1
curl -L https://github.com/LeoWoerteler/XQUTS/archive/master.tar.gz | tar -xz -C ./XPathTest/assets/XQUTS --strip-components=1
unzip -q ./XPathTest/assets/QT3TS/xqueryx.zip -d ./XPathTest/assets/QT3TS/
