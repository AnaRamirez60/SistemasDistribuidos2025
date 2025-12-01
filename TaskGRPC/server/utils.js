// cargar dependencias
const { ObjectId } = require("mongodb");

// Convierte un documento de MongoDB al formato de protobuf 
function docToProject(doc) {
  return {
    id: doc._id.toString(),
    title: doc.title,
    //valores por defecto si faltan
    summary: doc.summary || "",
    priority: doc.priority || 1,
    status: doc.status || "PENDING",
  };
}

// construir filtro MongoDB a partir de los filtros recibidos
function buildMongoFilters(filters = {}) {
  const mongoFilter = {};

  // procesar cada filtro
  for (const key of Object.keys(filters)) {
    const val = filters[key];

    // convertir tipos según el campo
    if (key === "priority") {
      mongoFilter.priority = parseInt(val, 10);
    } 
    else if (key === "id") {
      try {
        mongoFilter._id = new ObjectId(val);
      } catch {
        // por si se manda un id inválido 
        console.error("Id inválido ", val);
        continue;
      }
    }
    else if (key === "title") {
      // búsqueda por titulo
      mongoFilter.title = { $regex: val, $options: "i" };
    }
    else {
      // buscar los datos con los filtros
      mongoFilter[key] = val;
    }
  }

    // retornar el filtro 
  return mongoFilter;
}

// exportar funciones para ser utilizadas por index.js
module.exports = {
  docToProject,
  buildMongoFilters
};