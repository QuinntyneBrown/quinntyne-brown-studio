export default async function teardown() {
  const response = await fetch('http://127.0.0.1:4390/__test/shutdown', {
    method: 'POST',
    headers: { 'X-Reckoner-Test-Run': process.env.RECKONER_TEST_RUN_ID! },
    signal: AbortSignal.timeout(30000),
  }).catch(error => {
    if (error.cause?.code === 'ECONNREFUSED') return undefined;
    throw error;
  });
  if (response && !response.ok) throw new Error(`Reckoner test database cleanup failed: ${response.status}`);
}
