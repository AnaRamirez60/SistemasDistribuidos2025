// servidor de base de datos MongoDB
const { MongoClient } = require("mongodb");
const MONGO_URI = process.env.MONGO_URI || "mongodb://mongodb:27017";
const client = new MongoClient(MONGO_URI);

// colección de proyectos
let projectsColl;
// inicializar conexión, colección y tabla
async function initDb() {
  await client.connect();
  const db = client.db("projectdb");
  projectsColl = db.collection("projects");
  // índice único en title 
  await projectsColl.createIndex({ title: 1 }, { unique: true });
  console.log("MongoDB conectado.");
}
// obtener colección
function getCollection() {
  // asegurar inicialización
  if (!projectsColl) throw new Error("DB error");
  return projectsColl;
}

// exportar funciones de inicialización y obtención de colección
module.exports = { initDb, getCollection };
