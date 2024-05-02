def nugetBadge = addEmbeddableBadgeConfiguration(id: "nugetBadge", subject: "nuget", color: "blue")
pipeline {
    agent { label 'windows' }
    environment {
        GIT_VERSION = """${bat(
            returnStdout: true,
            script: '@git describe --tags'
        ).trim()}"""
        GIT_LAST_TAG = """${bat(
            returnStdout: true,
            script: '@git describe --tags --abbrev=0'
        ).trim()}"""
        TAG_NAME = """${bat(
            returnStdout: true,
            script: '@git --no-pager tag --points-at HEAD'
        ).trim()}"""
    }
    stages {
        stage('Build') {
            options { skipDefaultCheckout() }
            steps {
                script {
                    nugetBadge.setStatus("${GIT_LAST_TAG}")
                }
                bat "if exist artifacts rmdir artifacts /s /q"
                bat "mkdir artifacts"
                bat "mkdir artifacts\\reports"
                bat "dotnet build"
            }
        }
        stage('Unit tests') {
            options { skipDefaultCheckout() }
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
            options { skipDefaultCheckout() }
            steps {
                bat "dotnet stryker -c 4 -O artifacts"
                dir('artifacts/reports') {
                    bat "move /y mutation-tests.html Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.html"
                    bat "tar -a -c -f Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.zip Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.html"
                }
                archiveArtifacts artifacts: "artifacts/reports/Ixnas.AltchaNet-${GIT_VERSION}-mutation-tests.zip"
            }
        }
        stage('Package') {
            options { skipDefaultCheckout() }
            steps {
                bat "dotnet pack Ixnas.AltchaNet -o artifacts\\ -p:PackageVersion=${GIT_VERSION}"
                archiveArtifacts artifacts: "artifacts/Ixnas.AltchaNet.${GIT_VERSION}.nupkg"
            }
        }
        stage('Publish') {
            options { skipDefaultCheckout() }
            when {
                buildingTag()
            }
            steps {
                bat "dotnet nuget push artifacts\\Ixnas.AltchaNet.${GIT_VERSION}.nupkg --api-key ${NUGET_API_KEY} --source https://api.nuget.org/v3/index.json"
            }
        }
    }
}
