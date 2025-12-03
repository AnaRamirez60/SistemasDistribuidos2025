// Inicializacion de la base de datos y colecciones
db = db.getSiblingDB("projectsdb");

db.createCollection("projects", {
  validator: {
    $and: [
      {title: { $type: "string", $exists: true } },
      {priority: { $type: "int", $gte: 1, $lte: 5 } },
      {status: {$type: "string", $exists: true} },
      {summary: {$type: "string", $exists: true} }
    ]
  }
});

db.projects.createIndex({ title: 1 }, { unique: true });
print("init mongo");