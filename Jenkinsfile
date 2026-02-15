pipeline {
    agent any

        parameters {
        string(
            name: 'DEPLOY_VERSION',
            defaultValue: '',
            description: 'Docker image version to deploy (leave empty to deploy current build)'
        )
    }
environment {
    EFFECTIVE_VERSION = "${params.DEPLOY_VERSION?.trim() ? params.DEPLOY_VERSION : BUILD_NUMBER}"
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


stage('Resolve Versions') {
    steps {
        script {
            if (currentBuild.previousSuccessfulBuild) {
                env.ROLLBACK_VERSION = currentBuild.previousSuccessfulBuild.number.toString()
            } else {
                env.ROLLBACK_VERSION = ''
            }

            echo "Deploy version     : ${env.EFFECTIVE_VERSION}"
            echo "Rollback version   : ${env.ROLLBACK_VERSION ?: 'none'}"
        }
    }
}

stage('Deploy & Verify') {
    steps {
        script {
            try {
                sh '''
                  echo "Deploying version: ${EFFECTIVE_VERSION}"

                  docker stop my-web-app || true
                  docker rm my-web-app || true

                  docker pull ipushprajmishra/my-web-app:${EFFECTIVE_VERSION}

                  docker run -d \
                    --name my-web-app \
                    --restart always \
                    -p 8090:8080 \
                    -e APP_VERSION=${EFFECTIVE_VERSION} \
                    ipushprajmishra/my-web-app:${EFFECTIVE_VERSION}

                  echo "Waiting for app startup..."
                  sleep 10

                  echo "Running health check..."
                  STATUS=$(curl -s -o /dev/null -w "%{http_code}" http://localhost:8090/WeatherForecast/Health)

                  if [ "$STATUS" != "200" ]; then
                    echo "Health check failed with status $STATUS"
                    exit 1
                  fi

                  echo "Health check passed"
                '''
            }
            catch (Exception e) {
                echo "Deployment failed. Rolling back to last successful build: $ROLLBACK_VERSION"

                sh '''
                  docker stop my-web-app || true
                  docker rm my-web-app || true

                  docker pull ipushprajmishra/my-web-app:$ROLLBACK_VERSION

                  docker run -d \
                    --name my-web-app \
                    --restart always \
                    -p 8090:8080 \
                    -e APP_VERSION=$ROLLBACK_VERSION \
                    ipushprajmishra/my-web-app:$ROLLBACK_VERSION
                '''

                error("Deployment failed. Rolled back to $ROLLBACK_VERSION")
            }
        }
    }
}


stage('Show Versions') {
    steps {
        sh '''
          echo "Current build number: ${BUILD_NUMBER}"
          echo "Last successful build: $ROLLBACK_VERSION"
        '''
    }
}

    }
}
