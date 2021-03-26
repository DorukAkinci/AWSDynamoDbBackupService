
pipeline {
    agent {
        docker {
            label 'master'
            image 'mcr.microsoft.com/dotnet/core/sdk:3.1' 
        }
    }
    
    tools {
        git 'git'
    }
    
    parameters { 
          string(name: 'REGION', defaultValue: 'eu-west-1', description: 'AWS Region')
          string(name: 'PROFILE', defaultValue: 'default', description: 'AWS Profile')
    }
    
    environment{
        HOME = '/tmp'
        DOTNET_CLI_HOME = "/tmp/DOTNET_CLI_HOME"
    }
    
    stages {
        stage('Info') {
           steps {
               sh 'env'
            }
        }
        
        stage('Clone Repo') {
            steps {
                echo 'Cloning Repo'
                sh 'rm -rf repo; mkdir repo'
                dir ('repo') {
                    git branch: "master",
                    //credentialsId: 'XXXXXX',
                    url: 'https://github.com/DorukAkinci/AWSDynamoDbBackupService'
                }
            }
        }

        stage('Build Repo') {
            steps {
                echo 'Building Repo'
                dir ('repo') {
                    sh "ls -la"
                    sh "dotnet restore"
                    sh "dotnet publish -c Release -o Release"
                }
            }
        }
        
        stage('Execute the Application') {
            steps {
                echo 'Executing the Application'
                dir ('repo') {
                    dir ('Release') {
                        sh "ls -la"
                        sh "./DynamoDbBackupService --region ${REGION} --profile ${PROFILE}"
                    }
                }
            }
        }
    }
}
