
CONFIG = ENV['CONFIG'] || 'Debug'

COVERAGE_DIR = 'Coverage'
ASSEMBLY_INFO = 'src/StateMechanic/Properties/AssemblyInfo.cs'
NUSPEC = 'NuGet/StateMechanic.nuspec'
CSPROJ = 'src/StateMechanic/StateMechanic.csproj'
MSBUILD = %q{C:\Program Files (x86)\MSBuild\12.0\Bin\MSBuild.exe}

GITLINK_REMOTE = 'https://github.com/canton7/StateMechanic'

directory COVERAGE_DIR

desc "Create NuGet package"
task :package do
  local_hash = `git rev-parse HEAD`.chomp
  sh "NuGet/GitLink.exe . -s #{local_hash} -u #{GITLINK_REMOTE} -f src/StateMechanic.sln -ignore StateMechanicUnitTests"
  Dir.chdir(File.dirname(NUSPEC)) do
    sh "nuget.exe pack #{File.basename(NUSPEC)}"
  end
end

desc "Bump version number"
task :version, [:version] do |t, args|
  parts = args[:version].split('.')
  parts << '0' if parts.length == 3
  version = parts.join('.')

  content = IO.read(ASSEMBLY_INFO)
  content[/^\[assembly: AssemblyVersion\(\"(.+?)\"\)\]/, 1] = version
  content[/^\[assembly: AssemblyFileVersion\(\"(.+?)\"\)\]/, 1] = version
  File.open(ASSEMBLY_INFO, 'w'){ |f| f.write(content) }

  content = IO.read(NUSPEC)
  content[/<version>(.+?)<\/version>/, 1] = args[:version]
  File.open(NUSPEC, 'w'){ |f| f.write(content) }
end

desc "Build the project for release"
task :build do
  sh MSBUILD, CSPROJ, "/t:Clean;Rebuild", "/p:Configuration=Release", "/verbosity:normal"
end

task :test_environment do
  NUNIT_TOOLS = 'src/packages/NUnit.Runners.*/tools'
  NUNIT_CONSOLE = Dir[File.join(NUNIT_TOOLS, 'nunit-console.exe')].first
  NUNIT_EXE = Dir[File.join(NUNIT_TOOLS, 'nunit.exe')].first

  OPENCOVER_CONSOLE = Dir['src/packages/OpenCover.*/tools/OpenCover.Console.exe'].first
  REPORT_GENERATOR = Dir['src/packages/ReportGenerator.*/tools/ReportGenerator.exe'].first

  UNIT_TESTS_DLL = "src/StateMechanicUnitTests/bin/#{CONFIG}/StateMechanicUnitTests.dll"

  raise "NUnit.Runners not found. Restore NuGet packages" unless NUNIT_CONSOLE && NUNIT_EXE
  raise "OpenCover not found. Restore NuGet packages" unless OPENCOVER_CONSOLE
  raise "ReportGenerator not found. Restore NuGet packages" unless REPORT_GENERATOR
end

desc "Generate unit test code coverage reports for CONFIG (or Debug)"
task :cover => [:test_environment, COVERAGE_DIR] do
  coverage_file = File.join(COVERAGE_DIR, File.basename(UNIT_TESTS_DLL).ext('xml'))
  sh %Q{#{OPENCOVER_CONSOLE} -register:user -target:"#{NUNIT_CONSOLE}" -targetargs:"#{UNIT_TESTS_DLL} /noshadow" -filter:"+[StateMechanic]*" -output:"#{coverage_file}"}

  rm('TestResult.xml', :force => true)

  sh %Q{#{REPORT_GENERATOR} -reports:"#{coverage_file}" -targetdir:#{COVERAGE_DIR}}
end