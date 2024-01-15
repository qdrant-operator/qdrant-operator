namespace QdrantOperator
{
    public static class Dashboard
    {
        // "interval": "{0}"
        // "uid": "${{DS_PROMETHEUS}}"
        public static string DashboardJson =
"""
{{
  "annotations": {{
    "list": [
      {{
        "builtIn": 1,
        "datasource": {{
          "type": "grafana",
          "uid": "-- Grafana --"
        }},
        "enable": true,
        "hide": true,
        "iconColor": "rgba(0, 211, 255, 1)",
        "name": "Annotations & Alerts",
        "target": {{
          "limit": 100,
          "matchAny": false,
          "tags": [],
          "type": "dashboard"
        }},
        "type": "dashboard"
      }}
    ]
  }},
  "editable": true,
  "fiscalYearStartMonth": 0,
  "graphTooltip": 0,
  "id": 52,
  "links": [],
  "liveNow": false,
  "panels": [
    {{
      "collapsed": false,
      "gridPos": {{
        "h": 1,
        "w": 24,
        "x": 0,
        "y": 0
      }},
      "id": 23,
      "panels": [],
      "title": "Cluster",
      "type": "row"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "thresholds"
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 4,
        "x": 0,
        "y": 1
      }},
      "id": 12,
      "interval": "{0}",
      "options": {{
        "colorMode": "none",
        "graphMode": "none",
        "justifyMode": "auto",
        "orientation": "auto",
        "reduceOptions": {{
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        }},
        "textMode": "auto"
      }},
      "pluginVersion": "9.3.6",
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "max(cluster_term{{namespace=~\"$namespace\", service=~\"$service\"}})",
          "hide": false,
          "legendFormat": "{{{{pod}}}}",
          "range": true,
          "refId": "B"
        }}
      ],
      "title": "Term",
      "type": "stat"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "thresholds"
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 4,
        "x": 4,
        "y": 1
      }},
      "id": 27,
      "interval": "{0}",
      "options": {{
        "colorMode": "none",
        "graphMode": "none",
        "justifyMode": "auto",
        "orientation": "auto",
        "reduceOptions": {{
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        }},
        "textMode": "auto"
      }},
      "pluginVersion": "9.3.6",
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "max(cluster_peers_total{{namespace=~\"$namespace\", service=~\"$service\"}})",
          "hide": false,
          "legendFormat": "{{{{pod}}}}",
          "range": true,
          "refId": "B"
        }}
      ],
      "title": "Peers",
      "type": "stat"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "thresholds"
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 4,
        "x": 8,
        "y": 1
      }},
      "id": 15,
      "interval": "{0}",
      "options": {{
        "colorMode": "none",
        "graphMode": "none",
        "justifyMode": "auto",
        "orientation": "auto",
        "reduceOptions": {{
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        }},
        "textMode": "auto"
      }},
      "pluginVersion": "9.3.6",
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(cluster_voter{{namespace=~\"$namespace\", service=~\"$service\"}})",
          "hide": false,
          "legendFormat": "{{{{pod}}}}",
          "range": true,
          "refId": "B"
        }}
      ],
      "title": "Voters",
      "type": "stat"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "thresholds"
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 4,
        "x": 12,
        "y": 1
      }},
      "id": 28,
      "interval": "{0}",
      "options": {{
        "colorMode": "none",
        "graphMode": "none",
        "justifyMode": "auto",
        "orientation": "auto",
        "reduceOptions": {{
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        }},
        "textMode": "auto"
      }},
      "pluginVersion": "9.3.6",
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(cluster_pending_operations_total{{namespace=~\"$namespace\", service=~\"$service\"}})",
          "hide": false,
          "legendFormat": "{{{{pod}}}}",
          "range": true,
          "refId": "B"
        }}
      ],
      "title": "Pending Operations",
      "type": "stat"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "thresholds"
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 4,
        "x": 16,
        "y": 1
      }},
      "id": 13,
      "interval": "{0}",
      "options": {{
        "colorMode": "none",
        "graphMode": "none",
        "justifyMode": "auto",
        "orientation": "auto",
        "reduceOptions": {{
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        }},
        "textMode": "auto"
      }},
      "pluginVersion": "9.3.6",
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "max(cluster_commit{{namespace=~\"$namespace\", service=~\"$service\"}})",
          "legendFormat": "{{{{pod}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "cluster_commit",
      "type": "stat"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "thresholds"
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 4,
        "x": 20,
        "y": 1
      }},
      "id": 3,
      "interval": "{0}",
      "options": {{
        "colorMode": "none",
        "graphMode": "none",
        "justifyMode": "auto",
        "orientation": "auto",
        "reduceOptions": {{
          "calcs": [
            "lastNotNull"
          ],
          "fields": "",
          "values": false
        }},
        "textMode": "auto"
      }},
      "pluginVersion": "9.3.6",
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "max(collections_total{{namespace=~\"$namespace\", service=~\"$service\"}}) by (service)",
          "legendFormat": "{{{{service}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Collections",
      "type": "stat"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 12,
        "x": 0,
        "y": 8
      }},
      "id": 11,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [],
          "displayMode": "list",
          "placement": "bottom",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "max(cluster_peers_total{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Cluster Peers",
      "type": "timeseries"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 7,
        "w": 12,
        "x": 12,
        "y": 8
      }},
      "id": 14,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [],
          "displayMode": "list",
          "placement": "bottom",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(rate(cluster_pending_operations_total{{namespace=~\"$namespace\", service=~\"$service\"}}[$__rate_interval])) by (namespace, service)",
          "hide": false,
          "interval": "{0}",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Pending Operations",
      "type": "timeseries"
    }},
    {{
      "collapsed": false,
      "gridPos": {{
        "h": 1,
        "w": 24,
        "x": 0,
        "y": 15
      }},
      "id": 21,
      "panels": [],
      "title": "Collections",
      "type": "row"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 9,
        "w": 12,
        "x": 0,
        "y": 16
      }},
      "id": 2,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [],
          "displayMode": "list",
          "placement": "bottom",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(collections_vector_total{{namespace=~\"$namespace\", service=~\"$service\"}}) by(namespace, service, pod)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Vectors",
      "type": "timeseries"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 9,
        "w": 12,
        "x": 12,
        "y": 16
      }},
      "id": 26,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [],
          "displayMode": "list",
          "placement": "bottom",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(collections_vector_total{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Collection Vectors Total",
      "type": "timeseries"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 8,
        "w": 12,
        "x": 0,
        "y": 25
      }},
      "id": 4,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [],
          "displayMode": "list",
          "placement": "bottom",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "max(collections_full_total{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Collections Full Total",
      "type": "timeseries"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 8,
        "w": 12,
        "x": 12,
        "y": 25
      }},
      "id": 5,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [],
          "displayMode": "list",
          "placement": "bottom",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(collections_aggregated_total{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}",
          "range": true,
          "refId": "A"
        }}
      ],
      "title": "Collections Aggregated",
      "type": "timeseries"
    }},
    {{
      "collapsed": false,
      "gridPos": {{
        "h": 1,
        "w": 24,
        "x": 0,
        "y": 33
      }},
      "id": 19,
      "panels": [],
      "title": "gRPC API",
      "type": "row"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 9,
        "w": 24,
        "x": 0,
        "y": 34
      }},
      "id": 7,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [
            "mean"
          ],
          "displayMode": "table",
          "placement": "right",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(grpc_responses_avg_duration_seconds{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service, pod) != 0",
          "legendFormat": "{{{{namespace}}}}/{{{{ service }}}}/{{{{ pod }}}} avg",
          "range": true,
          "refId": "A"
        }},
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(grpc_responses_min_duration_seconds{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service, pod) != 0",
          "hide": false,
          "legendFormat": "{{{{namespace}}}}/{{{{ service }}}}/{{{{ pod }}}} min",
          "range": true,
          "refId": "B"
        }},
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(grpc_responses_max_duration_seconds{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service, pod) != 0",
          "hide": false,
          "legendFormat": "{{{{namespace}}}}/{{{{ service }}}}/{{{{ pod }}}} max",
          "range": true,
          "refId": "C"
        }}
      ],
      "title": "gRPC API responses duration",
      "type": "timeseries"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 9,
        "w": 24,
        "x": 0,
        "y": 43
      }},
      "id": 25,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [
            "lastNotNull"
          ],
          "displayMode": "table",
          "placement": "right",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(irate(grpc_responses_total{{namespace=~\"$namespace\", service=~\"$service\"}}[$__rate_interval])) by (namespace, service, pod)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} success",
          "range": true,
          "refId": "A"
        }},
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "sum(irate(grpc_responses_fail_total{{namespace=~\"$namespace\", service=~\"$service\"}}[$__rate_interval])) by (namespace, service, pod)",
          "hide": false,
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} failed",
          "range": true,
          "refId": "B"
        }}
      ],
      "title": "gRPC API RPS",
      "type": "timeseries"
    }},
    {{
      "collapsed": false,
      "gridPos": {{
        "h": 1,
        "w": 24,
        "x": 0,
        "y": 52
      }},
      "id": 17,
      "panels": [],
      "title": "REST API",
      "type": "row"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 8,
        "w": 24,
        "x": 0,
        "y": 53
      }},
      "id": 6,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [
            "lastNotNull"
          ],
          "displayMode": "table",
          "placement": "right",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(rest_responses_avg_duration_seconds{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service, pod)",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} avg",
          "range": true,
          "refId": "A"
        }},
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(rest_responses_min_duration_seconds{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service, pod)",
          "hide": false,
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} min",
          "range": true,
          "refId": "B"
        }},
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(rest_responses_max_duration_seconds{{namespace=~\"$namespace\", service=~\"$service\"}}) by (namespace, service, pod)",
          "hide": false,
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} max",
          "range": true,
          "refId": "C"
        }}
      ],
      "title": "REST API responses duration",
      "type": "timeseries"
    }},
    {{
      "datasource": {{
        "type": "prometheus",
        "uid": "${{DS_PROMETHEUS}}"
      }},
      "fieldConfig": {{
        "defaults": {{
          "color": {{
            "mode": "palette-classic"
          }},
          "custom": {{
            "axisCenteredZero": false,
            "axisColorMode": "text",
            "axisLabel": "",
            "axisPlacement": "auto",
            "barAlignment": 0,
            "drawStyle": "line",
            "fillOpacity": 0,
            "gradientMode": "none",
            "hideFrom": {{
              "legend": false,
              "tooltip": false,
              "viz": false
            }},
            "lineInterpolation": "linear",
            "lineWidth": 1,
            "pointSize": 5,
            "scaleDistribution": {{
              "type": "linear"
            }},
            "showPoints": "auto",
            "spanNulls": false,
            "stacking": {{
              "group": "A",
              "mode": "none"
            }},
            "thresholdsStyle": {{
              "mode": "off"
            }}
          }},
          "mappings": [],
          "thresholds": {{
            "mode": "absolute",
            "steps": [
              {{
                "color": "green",
                "value": null
              }},
              {{
                "color": "red",
                "value": 80
              }}
            ]
          }}
        }},
        "overrides": []
      }},
      "gridPos": {{
        "h": 8,
        "w": 24,
        "x": 0,
        "y": 61
      }},
      "id": 10,
      "interval": "{0}",
      "options": {{
        "legend": {{
          "calcs": [
            "lastNotNull"
          ],
          "displayMode": "table",
          "placement": "right",
          "showLegend": true
        }},
        "tooltip": {{
          "mode": "single",
          "sort": "none"
        }}
      }},
      "targets": [
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(irate(rest_responses_total{{namespace=~\"$namespace\", service=~\"$service\"}}[$__rate_interval])) by (namespace, service, pod) ",
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} success",
          "range": true,
          "refId": "A"
        }},
        {{
          "datasource": {{
            "type": "prometheus",
            "uid": "${{DS_PROMETHEUS}}"
          }},
          "editorMode": "code",
          "expr": "avg(irate(rest_responses_fail_total{{namespace=~\"$namespace\", service=~\"$service\"}}[$__rate_interval])) by (namespace, service, pod) ",
          "hide": false,
          "legendFormat": "{{{{namespace}}}}/{{{{service}}}}/{{{{pod}}}} failed",
          "range": true,
          "refId": "B"
        }}
      ],
      "title": "REST API RPS",
      "type": "timeseries"
    }}
  ],
  "refresh": false,
  "schemaVersion": 37,
  "style": "dark",
  "tags": [],
  "templating": {{
    "list": [
      {{
        "current": {{
          "selected": true,
          "text": "qdrant",
          "value": "qdrant"
        }},
        "datasource": {{
          "type": "prometheus",
          "uid": "${{DS_PROMETHEUS}}"
        }},
        "definition": "label_values(app_info, namespace)",
        "hide": 0,
        "includeAll": false,
        "label": "namespace",
        "multi": false,
        "name": "namespace",
        "options": [],
        "query": {{
          "query": "label_values(app_info, namespace)",
          "refId": "StandardVariableQuery"
        }},
        "refresh": 1,
        "regex": "",
        "skipUrlSync": false,
        "sort": 0,
        "type": "query"
      }},
      {{
        "current": {{
          "selected": true,
          "text": "qdrant-instep",
          "value": "qdrant-instep"
        }},
        "datasource": {{
          "type": "prometheus",
          "uid": "${{DS_PROMETHEUS}}"
        }},
        "definition": "label_values(app_info{{namespace=~\"$namespace\"}}, service)",
        "hide": 0,
        "includeAll": false,
        "label": "service",
        "multi": false,
        "name": "service",
        "options": [],
        "query": {{
          "query": "label_values(app_info{{namespace=~\"$namespace\"}}, service)",
          "refId": "StandardVariableQuery"
        }},
        "refresh": 1,
        "regex": "",
        "skipUrlSync": false,
        "sort": 0,
        "type": "query"
      }},
      {{
        "current": {{
          "selected": true,
          "text": [
            "All"
          ],
          "value": [
            "$__all"
          ]
        }},
        "datasource": {{
          "type": "prometheus",
          "uid": "${{DS_PROMETHEUS}}"
        }},
        "definition": "label_values(app_info{{namespace=~\"$namespace\"}}, pod)",
        "hide": 0,
        "includeAll": true,
        "label": "instance",
        "multi": true,
        "name": "instance",
        "options": [],
        "query": {{
          "query": "label_values(app_info{{namespace=~\"$namespace\"}}, pod)",
          "refId": "StandardVariableQuery"
        }},
        "refresh": 1,
        "regex": "",
        "skipUrlSync": false,
        "sort": 0,
        "type": "query"
      }}
    ]
  }},
  "time": {{
    "from": "now-1h",
    "to": "now"
  }},
  "timepicker": {{}},
  "timezone": "",
  "title": "Qdrant Cluster Dashboard",
  "uid": "{1}",
  "version": 1,
  "weekStart": ""
}}
""";
    }
}
