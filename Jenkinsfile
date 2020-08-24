
properties([
    pipelineTriggers([
        [
            $class: 'BitBucketPPRTrigger',
            triggers : [
                [
                    $class: 'BitBucketPPRPullRequestTriggerFilter',
                    actionFilter: [$class: 'BitBucketPPRPullRequestMergedActionFilter']
                ],
				[
					$class: 'BitBucketPPRRepositoryTriggerFilter',
					actionFilter: [
						$class: 'BitBucketPPRRepositoryPushActionFilter',
						triggerAlsoIfNothingChanged: true,
						triggerAlsoIfTagPush: false,
						allowedBranches: 'master'
					]
				]
            ]
        ]
    ])
])

version = ''
projectVersion = ''
commit_id = ''
sourceBranch = 'master'
targetBranch = 'master'
branchname = 'r1-automation-database-core'
giturl = "git@bitbucket.org:r1rcm/${branchname}.git"
gitCredentialsId = 'git-r1rcm-com'
	
	pipeline {
	   agent any
		options {
			timestamps()
		}
		environment {
			SONAR_HOME = tool name: 'SonarMSCore', type: 'hudson.plugins.sonar.MsBuildSQRunnerInstallation'
		}
		parameters { 
			string(name: 'FROM_BRANCH', defaultValue: '', description: 'Manual build against specific branch (blank for default):')
			string(name: 'FROM_TAG', defaultValue: '', description: 'Manual build against specific tag (blank for default):')
			string(name:'SonarQubeProjectKey',defaultValue:'', description:'Add the Project Key of sonarqube project (blank for default):')
		}
	   stages {
			stage('Clean Workspace') {
			   steps{
				 cleanWs() 
			   }
			}			
			stage('Build'){
				when { expression { !is_pullrequest() } }
				stages {
					stage('Checkout Code') {
						steps {
							script {
								sourceBranch = get_sourceBranch()
								echo "Checking out: ${sourceBranch}"
							}

							checkout([
								$class: 'GitSCM',
								branches: [[name: sourceBranch]],
								doGenerateSubmoduleConfigurations: false,
								userRemoteConfigs: [[
									credentialsId: gitCredentialsId,
									url: giturl
								]]
							])

							script {
								commit_id = get_commit_id()
								echo "commit_id: ${commit_id}"				
							}
							
							bitbucketStatusNotify(
							buildState: 'INPROGRESS',
							buildKey: 'build',
							buildName: 'Build',
							repoSlug: branchname,
							commitId: "${commit_id}",
							buildDescription: 'Starting the build.')
						}
					}
					//stage('Restore Packages') {
						//steps {
						//	restore_package()					
						//}
					//}
					stage('Clean Build') {
					   steps {
							clean_build()
					   }
					}					
					stage('Build') {
					   steps {
							run_build()
					   }
					}				
					stage ('Static Code Analysis') {
						steps {
							static_code_analysis()
						}
					}					
				}
			}			
	    }
		post {	
		 always {
                               echo 'Build Finished'                              
                               //Setting up the environment variable (for sonarqube project key) to be passed into the email template
                               script{
                                      env.projectKeyValue = "R1_Automation_Database_Core"
                               }
                       }
			success {
				script {
					echo 'Build was successful'
					key = is_pullrequest() ? 'pullrequest' : 'build'
					name = is_pullrequest() ? 'Pullrequest' : 'Build'
					description = is_pullrequest() ? 'Pull-request successful' : 'Build successful'

					bitbucketStatusNotify(
						buildState: 'SUCCESSFUL',
						buildKey: key,
						buildName: name,
						repoSlug: branchname,
						commitId: "${commit_id}",
						buildDescription: description)

					notify_recipients(true)
				}
			}
			fixed {
				echo 'Build is now stable'
				notify_recipients(true)
			}
			changed {
				echo 'Build status changed'
			}
			aborted {
				echo 'Build was manually aborted'
				notify_recipients(true)
			}
			failure {
				script {
					echo "Build Failed"
					key = is_pullrequest() ? 'pullrequest' : 'build'
					name = is_pullrequest() ? 'Pullrequest' : 'Build'
					description = is_pullrequest() ? 'Pull-request failed' : 'Build failed'

					bitbucketStatusNotify(
						buildState: 'FAILURE',
						buildKey: key,
						buildName: name,
						repoSlug: branchname,
						commitId: "${commit_id}",
						buildDescription: description)

					notify_recipients(false)
				}
			}
			unstable {
				echo 'Build is Unstable'
			}
		}	
	}
	
def get_sourceBranch() {
	if (params.FROM_BRANCH != '') {
		return "*/${params.FROM_BRANCH}"
	}

	if (params.FROM_TAG != '') {
		return "refs/tags/${params.FROM_TAG}"
	}

	// detect branch from environment
	def gitBranch = env.GIT_BRANCH

	// fall-back to master
	if (gitBranch == null) {
		gitBranch = 'master'
	}
	gitBranch = gitBranch.replace('origin/', '')
	echo "gitBranch: ${gitBranch}"

	return "*/${gitBranch}"
}

def restore_package(){
	def code = bat label: '', returnStatus: true, script: 'dotnet restore R1.Automation.Database.core.sln --configfile=Nuget.Config.xml'
	echo "Windows batch scripts exit code: ${code}"
	if (code != 0) {
		error('Restore failed')
	}
}

def clean_build(){
	def code = bat label: '', returnStatus: true, script: 'dotnet clean'
	echo "Windows batch scripts exit code: ${code}"
	if (code != 0) {
		error('Clean Build failed')
	}	
}

def run_build(){
	//Run Build
	def code = bat label: '',returnStatus: true, script: 'dotnet build -c Release'
	echo "Windows batch scripts exit code: ${code}"
	if (code != 0) {
		error('Build failed')
	}
	
	//Create Build Folder if it doesn't exist
	def code_check = bat label: '', script: 'if exist build ( echo build folder exists ) else ( mkdir build && echo build folder created)'
	
	//publish build
	def code_pub = bat label: '', returnStatus: true, script: 'dotnet publish -c Release --no-build -o build\\R1AutomationDatabaseCore_Published'
	if (code_pub != 0) {
		error('Publish Failed')
	}
	else{
		echo "publish successful at location ${env:WORKSPACE}\\build\\R1AutomationDatabaseCore_Published"
	}
}



def static_code_analysis() {
	def sonar_url = "https://sonarqube.r1rcm.com"
	def sonar_login = "bef0e64482d16bd04a2e5bde0b109949a32a1b65"

	echo "SONAR_HOME: ${env.SONAR_HOME}"

	def code = bat label: '', returnStatus: true, script: "dotnet ${SONAR_HOME}\\SonarScanner.MSBuild.dll begin /k:R1_Automation_Database_Core /n:R1_Automation_Database_Core /d:sonar.host.url=\"${sonar_url}\" /d:sonar.login=\"${sonar_login}\" /d:sonar.cs.dotcover.reportsPaths=\"R1_Automation_Database_core-coverage-report.html\""
	if (code != 0) {
		error('Failed to begin static code analysis')
	}

	code = bat label: '', returnStatus: true, script: 'dotnet build --no-incremental R1.Automation.Database.core.sln'
	if (code != 0) {
		error('Failed to rebuild for static code analysis')
	}

	code = bat label: '', returnStatus: true, script: "dotnet ${SONAR_HOME}\\SonarScanner.MSBuild.dll end /d:sonar.login=\"${sonar_login}\""
	if (code != 0) {
		error('Failed to end static code analysis run')
	}
}



def is_webhook() {
	return env.BITBUCKET_SOURCE_BRANCH
}

def is_pullrequest() {
	return env.BITBUCKET_PULL_REQUEST_ID
}

def get_commit_id() {
	if (is_webhook() && is_pullrequest()) {
		def payload = readJSON file: '', text: env.BITBUCKET_PAYLOAD
		echo "payload: ${payload}"

		def hash = payload['pullrequest']['source']['commit']['hash']
		return hash
	}

	def gitCommit = bat(returnStdout: true, script: '@echo off & git rev-parse HEAD').trim()
	echo "gitCommit: ${gitCommit}"
	return gitCommit
}

def notify_recipients(isSuccess = true) {
	def recipientProviders = [ requestor() ];
	def directRecipients = ""
	if (isSuccess) {
		directRecipients = "NAgarwal@R1RCM.COM,VShukla11@R1RCM.COM"
	}
	else {
		recipientProviders.add(culprits())
		recipientProviders.add(developers())
	}
    emailext (
            to: "${directRecipients}",
			recipientProviders: recipientProviders,
			compressLog: true,
            subject: '$PROJECT_NAME - Build # $BUILD_NUMBER - $BUILD_STATUS!',
			body:  '''${SCRIPT, template="r1-jenkins-email-html.template"}'''
        )
}