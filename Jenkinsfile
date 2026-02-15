pipeline {
    agent any

        parameters {
        string(
            name: 'DEPLOY_VERSION',
            defaultValue: '',
            description: 'Docker image version to deploy (leave empty to deploy current build)'
        )
    }

    stages {

        stage('Checkout Code') {
            steps {
                git branch: 'master',
                    credentialsId: 'GithubCreds',
                    url: 'https://github.com/ipushprajmishra/my-web-app.git'
            }
        }

        stage('Verify Workspace') {
            steps {
                sh 'pwd'
                sh 'ls -la'
            }
        }
        stage('Inspect Workspace') {
    steps {
        sh '''
          echo "Workspace path:"
          pwd

          echo "Files:"
          ls -la

          echo "Git status:"
          git status

          echo "Current branch:"
          git branch --show-current
        '''
    }
}

stage('Docker Sanity Check') {
    steps {
        sh '''
          echo "Docker version:"
          docker version

          echo "Running containers:"
          docker ps
        '''
    }
}

stage('Build Docker Image') {
    steps {
        sh '''
          echo "Building Docker image..."
          docker build --memory=512m -t my-web-app:test .
        '''
    }
}
stage('Tag & Push Image') {
    steps {
        withCredentials([usernamePassword(
            credentialsId: 'dockerhub-creds',
            usernameVariable: 'DOCKER_USER',
            passwordVariable: 'DOCKER_PASS'
        )]) {
            sh '''
              echo "Logging in to Docker Hub"
              echo "$DOCKER_PASS" | docker login -u "$DOCKER_USER" --password-stdin

              echo "Tagging image"
              docker tag my-web-app:test ipushprajmishra/my-web-app:${BUILD_NUMBER}
              docker tag my-web-app:test ipushprajmishra/my-web-app:latest

              echo "Pushing image"
              docker push ipushprajmishra/my-web-app:${BUILD_NUMBER}
              docker push ipushprajmishra/my-web-app:latest
            '''
        }
    }
}
stage('Resolve Deploy Version') {
    steps {
        sh '''
          if [ -z "$DEPLOY_VERSION" ]; then
            echo "No DEPLOY_VERSION provided. Using current build: ${BUILD_NUMBER}"
          else
            echo "DEPLOY_VERSION provided. Deploying version: ${DEPLOY_VERSION}"
          fi
        '''
    }
}
stage('Deploy Container') {
    steps {
        sh '''
          echo "Stopping old container (if exists)"
          docker stop my-web-app || true
          docker rm my-web-app || true

          echo "Pulling image from Docker Hub"
          docker pull ipushprajmishra/my-web-app:${BUILD_NUMBER}

          echo "Running new container"
          docker run -d \
            --name my-web-app \
            --restart always \
            -p 8090:8080 \
            ipushprajmishra/my-web-app:${BUILD_NUMBER}
        '''
    }
}

stage('Show Versions') {
    steps {
        sh '''
          echo "Current build number: ${BUILD_NUMBER}"
          echo "Last successful build: ${LAST_SUCCESSFUL_BUILD_NUMBER}"
        '''
    }
}

    }
}
