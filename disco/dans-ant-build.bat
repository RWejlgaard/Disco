set VERSION=%1
set MW_HOME=C:\Oracle
set JDEV_HOME=%MW_HOME%\jdeveloper
set ANT_HOME=%MW_HOME%\oracle_common\modules\org.apache.ant_1.9.2
set PATH=%ANT_HOME%\bin;%PATH%
set CLASSPATH=%JDEV_HOME%\jdev\lib\ant-jdeveloper.jar;%CLASSPATH%

ant deploy -Ddansversion=%VERSION% -Doracle.middleware=%MW_HOME% -Doracle.home=%JDEV_HOME% -Doracle.jdeveloper.ant.library=%JDEV_HOME%/jdev/lib/ant-jdeveloper.jar -Doracle.jdeveloper.ojdeploy.path=%JDEV_HOME%/jdev/bin/ojdeploy.exe