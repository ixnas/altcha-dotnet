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
        MSBUILDDISABLENODEREUSE = "1"
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
                bat "dotnet test --no-build --logger:\"junit;LogFilePath=Ixnas.AltchaNet-${GIT_VERSION}-unit-tests-{framework}.xml\" --collect:\"XPlat Code Coverage\" --results-directory artifacts\\reports"
                bat "move /y Ixnas.AltchaNet.Tests\\Ixnas.AltchaNet-${GIT_VERSION}-unit-tests-*.xml artifacts\\reports\\"
                dir('artifacts/reports') {
                    bat "tar -a -c -f Ixnas.AltchaNet-${GIT_VERSION}-unit-tests.zip Ixnas.AltchaNet-${GIT_VERSION}-unit-tests-*.xml"
                }
                junit "artifacts/reports/Ixnas.AltchaNet-${GIT_VERSION}-unit-tests-*.xml"
                recordCoverage(tools: [[parser: 'COBERTURA', pattern: 'artifacts/reports/*/coverage.cobertura.xml']], sourceCodeRetention: 'EVERY_BUILD')
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
                bat "dotnet pack Ixnas.AltchaNet -o artifacts\\ -p:PackageVersion=${GIT_VERSION} -p:AssemblyVersion=${GIT_LAST_TAG} -p:InformationalVersion=${GIT_LAST_TAG} -p:ContinuousIntegrationBuild=true -p:EmbedUntrackedSources=true"
                archiveArtifacts artifacts: "artifacts/Ixnas.AltchaNet.${GIT_VERSION}.nupkg"
            }
        }
        stage('Publish') {
            options { skipDefaultCheckout() }
            when {
                buildingTag()
            }
            steps {
                withCredentials([string(credentialsId: "NUGET_API_KEY", variable: "TOKEN")]) {
                    bat 'dotnet nuget push artifacts\\Ixnas.AltchaNet.%GIT_VERSION%.nupkg --api-key %TOKEN% --source https://api.nuget.org/v3/index.json'
                }
            }
        }
    }
}
