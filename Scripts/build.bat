SET VERSION_NUMBER=%1
SET GIT_BRANCH=%2

IF "%VERSION_NUMBER%"=="" (ECHO "VERSION_NUMBER not set" && exit 1)
IF "%GIT_BRANCH%"=="" (ECHO "GIT_BRANCH not set" && exit 1)


GOTO :START

This is a build script that compiles builds and tests the Admo Application

Usage:
   myscript $VERSION_NUMBER $GIT_BRANCH

:START

ECHO %VERSION_NUMBER%
msbuild admo-kinect.sln /target:Clean /property:Configuration=Release
RD /s /q  AdmoInstaller\bin\Release
RD /s /q  AdmoInstaller\bin\dist
RD /s /q  Release
RD /s /q  TestResults
MKDIR  Release


msbuild admo-kinect.sln /property:Configuration=Release /p:AsmVersion=%VERSION_NUMBER%

REM "====== Tests ======"

vstest.console.exe /inIsolation /Platform:x64 /SETtings:AdmoTests\TestsSETtings.runSETtings AdmoTests/bin/Release/AdmoTests.dll /Logger:trx 

REM "Archiving installer and test results"
SET GIT_BRANCH_CLEAN=%GIT_BRANCH%
REM "Replace / with -"
SET GIT_BRANCH_CLEAN=%GIT_BRANCH_CLEAN:/=-%
REM "Remove the origin-"
SET GIT_BRANCH_CLEAN=%GIT_BRANCH_CLEAN:origin-=-%
REM "Remove branch name if master"
SET GIT_BRANCH_CLEAN=%GIT_BRANCH_CLEAN:-master=%


SET MSI_FILE=Admo%GIT_BRANCH_CLEAN%-%VERSION_NUMBER%.msi
SET MSI_PATH=Release\%MSI_FILE%

copy AdmoInstaller\bin\Release\AdmoInstaller.msi %MSI_PATH%


PUSHD Release
    ECHO "Found MSI called %MSI_FILE%"

    FOR /F " " %%i IN ('..\Tools\sha256sum.exe %MSI_FILE%') DO SET SHA256_CHECKSUM=%%i
    ECHO "SHA256_CHECKSUM is %SHA256_CHECKSUM%"

    FOR /F " " %%i IN ('..\Tools\md5sum.exe %MSI_FILE%') DO SET MD5_CHECKSUM=%%i
    ECHO "MD5_CHECKSUM is %MD5_CHECKSUM%"

    REM "Save the html page so we can view it"
    ECHO|SET /p="<html><body>" >index.html
    ECHO. >>index.html
    ECHO|SET /p="<a href='" >>index.html
    ECHO|SET /p="%MSI_FILE%'>%MSI_FILE%" >>index.html
    ECHO|SET /p="<a/><br/>" >>index.html
    ECHO. >>index.html
    ECHO|SET /p="VERSION_NUMBER: %VERSION_NUMBER%<br/>" >>index.html
    ECHO. >>index.html
    ECHO|SET /p="SHA256_CHECKSUM: %SHA256_CHECKSUM%<br/>" >>index.html
    ECHO. >>index.html
    ECHO|SET /p="MD5_CHECKSUM: %MD5_CHECKSUM%<br/>" >>index.html
    ECHO. >>index.html
    ECHO|SET /p="</body></html>" >>index.html


    ECHO "Publish the API Version with the name and checksums"
    ECHO|SET /p={"filename":"%MSI_FILE%" > version.json
    ECHO|SET /p=,"version":"%VERSION_NUMBER%" >> version.json
    ECHO|SET /p=,"sha256":"%SHA256_CHECKSUM%" >> version.json
    ECHO|SET /p=,"md5":"%MD5_CHECKSUM%" >> version.json
    ECHO|SET /p=} >> version.json
POPD ..