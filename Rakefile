
CONFIG = ENV['CONFIG'] || 'Debug'

COVERAGE_DIR = 'Coverage'

directory COVERAGE_DIR

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
  sh %Q{#{OPENCOVER_CONSOLE} -register:user -target:"#{NUNIT_CONSOLE}" -targetargs:"#{UNIT_TESTS_DLL} /noshadow" -output:"#{coverage_file}"}

  rm('TestResult.xml', :force => true)

  sh %Q{#{REPORT_GENERATOR} -reports:"#{coverage_file}" -targetdir:#{COVERAGE_DIR}}
end