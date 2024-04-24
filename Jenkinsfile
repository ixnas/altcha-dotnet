pipeline {
    agent none
    stages {
        stage('Build') {
            agent { label 'windows' }
            steps {
                bat "if exist artifacts rmdir artifacts /s /q"
                bat "mkdir artifacts"
                bat "mkdir artifacts\\reports"
                bat "dotnet build"
            }
        }
        stage('Unit tests') {
            agent { label 'windows' }
            options { skipDefaultCheckout() }
            environment {
                GIT_VERSION = """${bat(
                    returnStdout: true,
                    script: '@git describe --tags'
                ).trim()}"""
            }
            steps {
                bat "dotnet test --logger:\"junit;LogFilePath=unit-tests.xml\""
                bat "move /y Ixnas.AltchaNet.Tests\\unit-tests.xml artifacts\\reports\\Ixnas.AltchaNet-${GIT_VERSION}-unit-tests.xml"
                dir('artifacts/reports') {
                    bat "tar -a -c -f Ixnas.AltchaNet-${GIT_VERSION}-unit-tests.zip Ixnas.AltchaNet-${GIT_VERSION}-unit-tests.xml"
                }
                junit "artifacts/reports/Ixnas.AltchaNet-${GIT_VERSION}-unit-tests.xml"
                archiveArtifacts artifacts: "artifacts/reports/Ixnas.AltchaNet-${GIT_VERSION}-unit-tests.zip"
            }
        }
        stage('Mutation tests') {
            agent { label 'windows' }
            options { skipDefaultCheckout() }
            environment {
                GIT_VERSION = """${bat(
                    returnStdout: true,
                    script: '@git describe --tags'
                ).trim()}"""
            }
            steps {
                bat "dotnet stryker -O artifacts"
                dir('artifacts/reports') {
                    bat "move /y mutation-tests.html Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.html"
                    bat "tar -a -c -f Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.zip Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.html"
                }
                archiveArtifacts artifacts: "artifacts/reports/Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.zip"
            }
        }
        stage('Package (development)') {
            agent { label 'windows' }
            options { skipDefaultCheckout() }
            environment {
                GIT_VERSION = """${bat(
                    returnStdout: true,
                    script: '@git describe --tags'
                ).trim()}"""
            }
            steps {
                bat "dotnet pack Ixnas.AltchaNet -o artifacts\\ -p:PackageVersion=${GIT_VERSION}"
                archiveArtifacts artifacts: "artifacts/Ixnas.AltchaNet.${GIT_VERSION}.nupkg"
            }
        }
    }
}
